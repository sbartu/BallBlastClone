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
		if(gameStarted && ObjectPooler.sharedInstance.EnemiesDead()) 
		{
			Debug.Log("Level Over");
			NextLevel();
		}
	}

	void NextLevel () 
	{
		if(currentLevel > 0)
			Stats.sharedInstance.IncreaseStats();

		for (int i = 0; i < levels[currentLevel].balls.Count; i++) {
			StartCoroutine(SpawnBall(levels[currentLevel].balls[i]));
		}
		gameStarted = true;
		currentLevel++;
	}

	private IEnumerator SpawnBall (Ball ball) {
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
			enemy.transform.position = new Vector3(Random.value > 0.5f ? SpawnXPosMin : SpawnXPosMax, Random.Range(SpawnYPosMin, SpawnYPosMax), 0.0f);
			rigid.useGravity = true;

			Debug.Log("Ball Spawned!!!!");
            
        }
	}

	void LastLevelTotalHealth() 
	{
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

	Level CreateNewLevel() 
	{
		float levelHealthOffset = Random.Range(levelHealthMin, levelHealthMax);
		int totalHealth = Mathf.FloorToInt(LastTotalHealth * levelHealthOffset);

		int maxPerBall = Mathf.Floor(totalHealth / 4);
		
	}
}
