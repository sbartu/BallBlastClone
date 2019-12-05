using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgress : MonoBehaviour {

	public List<Level> levels;
	public int currentLevel = 0;
	public int SpawnXPosMax = 7;
	public int SpawnXPosMin = -7;
	public float SpawnYPosMax = 9f;
	public float SpawnYPosMin = 7f;
	private bool gameStarted = false;
	private int LastTotalHealth;
	private float levelHealthMax = 1.2f;
	private float levelHealthMin = 0.8f;

	void Start () {
		levels = Stats.sharedInstance.input.levels;
		LastLevelTotalHealth();
		NextLevel();
	}

	void Update () 
	{
		//Check when to advance to the next level.
		if(gameStarted && ObjectPooler.sharedInstance.EnemiesDead()) 
		{
			NextLevel();
		}
	}

	void NextLevel () 
	{
		//If given levels have ended, start randomizing from here on out.
		if(currentLevel > 4)
			CreateNewLevel();

		//Increase bullet count and bullet damage after the initial level.
		if(currentLevel > 0)
			Stats.sharedInstance.IncreaseStats();

		//Start a coroutine for each ball, spawn them after their given delay time.
		for (int i = 0; i < levels[currentLevel].balls.Count; i++) {
			StartCoroutine(SpawnBall(levels[currentLevel].balls[i]));
		}
		gameStarted = true;
		currentLevel++;
	}

	private IEnumerator SpawnBall (Ball ball) {
		//Get ball object from the object pool.
		GameObject enemy = ObjectPooler.sharedInstance.GetPooledObject("Enemy"); 
        if (enemy != null) {

			BallHealth enemyHealth = enemy.GetComponent<BallHealth>();
			Rigidbody rigid = enemy.GetComponent<Rigidbody>();
			Vector3 randomSize = Stats.sharedInstance.GetRandomSize();
			enemy.transform.localScale = randomSize;

			//The enemies are set active outside of the gamefield to mark them as in use.
			//Gravity is disabled to prevent them from falling for eternity before they are spawned.
			rigid.useGravity = false;
			enemy.SetActive(true);
			enemyHealth.health = ball.hp;
			enemyHealth.splits = ball.splits;

			yield return new WaitForSeconds(ball.delay);
			//After the respective delay, start the coroutine to move the ball
			//into the gamefield either from the left wall or the right wall,
			//chosen randomly.
			StartCoroutine(MoveToPosition(enemy, 2f, rigid));
        }
	}

	IEnumerator MoveToPosition(GameObject enemy, float duration, Rigidbody rigid)
	{
		//This coroutine is created to imitate the balls eemerging from the right or left wall
		//just like in the original game.
		//Randomize y-coordinate between given public max min values.
		Collider collision = enemy.GetComponent<Collider>();
		float randomValue = Random.value;

		//These are set as so since the ball is currently traveling inside the collision of the wall.
		collision.isTrigger = true;
		rigid.useGravity = false;

		enemy.transform.position = new Vector3(randomValue > 0.5f ? SpawnXPosMin - 2f : SpawnXPosMax + 2f, Random.Range(SpawnYPosMin, SpawnYPosMax), 0.0f);
		Vector3 target = new Vector3(randomValue > 0.5f ? SpawnXPosMin : SpawnXPosMax, Random.Range(SpawnYPosMin, SpawnYPosMax), 0.0f);

		float elapsedTime = 0;
		Vector3 startingPos  = enemy.transform.position;
		while (elapsedTime < duration)
		{
			enemy.transform.position = Vector3.Lerp(startingPos, target, (elapsedTime / duration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		//After the lerp ends, the trigger and gravity values are corrected.
		//A little boost is added as a force to propel the ball into the gamefield.
		collision.isTrigger = false;
		rigid.useGravity = true;
		rigid.AddForce(100f * (randomValue > 0.5f ? Vector3.right : Vector3.left));
	}

	void LastLevelTotalHealth() 
	{
		//Find the total health of Level 5 from the given json.
		int totalHealth = 0;
		List<Ball> balls = levels[4].balls;
		for(int i = 0; i < balls.Count; i++) {
			totalHealth += balls[i].hp;
			foreach(int hp in balls[i].splits) {
				totalHealth += hp;
			}
		}
		LastTotalHealth = totalHealth;
	}

	void CreateNewLevel() 
	{
		//Create a new randomized level based off of the total health from Level 5.
		//The total health is in between %80 and %120 of Level 5. Delays are the same.
		float levelHealthOffset = Random.Range(levelHealthMin, levelHealthMax);
		int totalHealth = Mathf.FloorToInt(LastTotalHealth * levelHealthOffset);

		//Split the health into 4 balls. Last ball gets remaining health.
		int maxPerBall = totalHealth / 4;
		int minPerBall = 2;
		int Ball1 = Random.Range(minPerBall, maxPerBall);
		int Ball2 = Random.Range(minPerBall, maxPerBall);
		int Ball3 = Random.Range(minPerBall, maxPerBall);
		int Ball4 = totalHealth - Ball1 - Ball2 - Ball3;

		//Ball health is shared with its splits.
		Ball1 = Ball1 / 2;
		Ball2 = Ball2 / 2;
		Ball3 = Ball3 / 2;
		Ball4 = Ball4 / 2;

		int Split1 = Ball1 / 2;
		int Split2 = Ball2 / 2;
		int Split3 = Ball3 / 2;
		int Split4 = Ball4 / 2;

		//If any extra health reamins, split them in equal but give the remainder of the operation
		//to the first ball (arbitrary).
		int RemainingHealth = totalHealth - Ball1 - Ball2 - Ball3 - Ball4 - (Split1 * 2) - (Split2 * 2) - (Split3 * 2) - (Split4 * 2);
		int Remainder = RemainingHealth % 4;
		RemainingHealth = RemainingHealth / 4;

		Ball1 += RemainingHealth + Remainder;
		Ball2 += RemainingHealth;
		Ball3 += RemainingHealth;
		Ball4 += RemainingHealth;

		//Initialize balls.
		Ball Ball1in = new Ball(Ball1, new List<int>() {Split1, Split1}, 0);
		Ball Ball2in = new Ball(Ball2, new List<int>() {Split2, Split2}, 4);
		Ball Ball3in = new Ball(Ball3, new List<int>() {Split3, Split3}, 10);
		Ball Ball4in = new Ball(Ball4, new List<int>() {Split4, Split4}, 12);

		//Create a new level with the newly randomized balls. Add the level to the levels list.
		Level newLevel = new Level(new List<Ball>() {Ball1in, Ball2in, Ball3in, Ball4in});
		levels.Add(newLevel);
	}
}
