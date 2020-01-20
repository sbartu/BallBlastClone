using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelProgress : MonoBehaviour {

	public static LevelProgress sharedInstance;
	public List<Level> levels;
	public int currentDamage = 0;
	public int totalDamage = 0;
	public float launchForce;
	public float proceduralIncrementer = 1.4f;
	public AudioClip battleBgm;

	private int currentLevel = 0;
	private float SpawnXPosMax = 2f;
	private float SpawnXPosMin = -2f;
	private float SpawnYPosMax = 9f;
	private float SpawnYPosMin = 6.5f;

	private GameObject startDisp;
	private GameObject winDisp;
	private GameObject loseDisp;
	private GameObject scoreDisp;
	private GameObject startRect;
	private Text scoreText;
	private Text winText;
	private Text loseText;
	private bool gameStarted = false;
	private bool gameEnded = false;
	private int lastTotalHealth;
	private int totalHealth = 0;
	private int totalBallNum = 0;

	void Start () 
	{
		FindTexts();
		levels = Stats.sharedInstance.input.levels;
		sharedInstance = this;
		//Store health of last level ahead of time
		//to use it for random level generation.
		lastTotalHealth = TotalHealth(4);
	}

	void FindTexts() 
	{
		//Find respective canvas objects.
		GameObject canvas = GameObject.Find("Canvas");
		startRect = canvas.transform.Find("GameStarter").gameObject;
		startDisp = canvas.transform.Find("StartText").gameObject;
		winDisp = canvas.transform.Find("WinDisplay").gameObject;
		scoreDisp = canvas.transform.Find("ScoreText").gameObject;
		scoreText = scoreDisp.GetComponent<Text>();
		winText = winDisp.GetComponentInChildren<Text>();
		loseDisp = canvas.transform.Find("LoseDisplay").gameObject;
		loseText = loseDisp.GetComponentInChildren<Text>();
	}

	public void DecreaseBallNum() 
	{
		//Decreases number of enemies left by 1. Ends level if it reaches 0.
		totalBallNum -= 1;
		if(gameStarted && totalBallNum <= 0) 
			{
				Time.timeScale = 0;
				DisplayWinScreen(true);
			}
	}

	public void UpdateScoreText() 
	{
		if(!gameEnded)
			scoreText.text = currentDamage + " / " + totalHealth;
	}

	public void StartGame () 
	{
		//Start the game when mouse left-click is registered.
		if (!gameStarted) {
			startRect.SetActive(false);
			startDisp.SetActive(false);

			AudioSource audio = Camera.main.GetComponent<AudioSource>();
			audio.clip = battleBgm;
			audio.Play();
			NextLevel();
			DisplayScore();
		}
	}

	public void NextLevel () 
	{
		//Reset current level damage and unpause game.
		currentDamage = 0;
		DisplayWinScreen(false); 
		Time.timeScale = 1;
		//If given levels have ended, start randomizing from here on out.
		if(currentLevel > 4)
			CreateNewLevel();

		totalHealth = TotalHealth(currentLevel);
		UpdateScoreText();

		//Increase bullet count and bullet damage after the initial level.
		if(currentLevel > 0)
			Stats.sharedInstance.IncreaseStats();

		//Start a coroutine for each ball, spawn them after their given delay time.
		for (int i = 0; i < levels[currentLevel].balls.Count; i++) {
			StartCoroutine(SpawnBall(levels[currentLevel].balls[i]));
		}
		SetTotalBallNum(currentLevel);
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
			enemy.layer = 11;
			enemyHealth.health = ball.hp;
			enemyHealth.splits = ball.splits;
			enemy.SetActive(true);

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
		enemy.layer = 10;
		rigid.AddForce(launchForce * (randomValue > 0.5f ? Vector3.right : Vector3.left));
	}

	void SetTotalBallNum(int levelNo) 
	{
		//Set the total number of balls (*3 since every ball has 2 splits) 
		//for the given levelNo from the given json.
		totalBallNum = levels[levelNo].balls.Count * 3;
	}

	int TotalHealth(int levelNo) 
	{
		//Find the total health for the given levelNo from the given json.
		int totalHealth = 0;
		List<Ball> balls = levels[levelNo].balls;
		for(int i = 0; i < balls.Count; i++) {
			totalHealth += balls[i].hp;
			foreach(int hp in balls[i].splits) {
				totalHealth += hp;
			}
		}
		return totalHealth;
	}

	void CreateNewLevel() 
	{
		float levelHealthMax = 1.2f;
		float levelHealthMin = 0.8f;
		float delayMax = 12;
		float delayMin = 0;
		float randomMin = 0.6f;
		float randomMax = 1.4f;
		//Create a new randomized level based off of the total health from Level 5.
		//The total health is in between %80 and %120 of Level 5. Delays are the same.
		float levelHealthOffset = Random.Range(levelHealthMin, levelHealthMax);
		int totalHealth = Mathf.FloorToInt(lastTotalHealth * levelHealthOffset);

		//Split the health into 4 balls. Last ball gets remaining health.
		int dividedHealth = totalHealth / 4;
		int ball1 = (int) (dividedHealth * Random.Range(randomMin, randomMax));
		int ball2 = (int) (dividedHealth * Random.Range(randomMin, randomMax));
		int ball3 = (int) (dividedHealth * Random.Range(randomMin, randomMax));
		int ball4 = (int) (dividedHealth * Random.Range(randomMin, randomMax));

		//Ball health is shared with its splits.
		ball1 = ball1 / 2;
		ball2 = ball2 / 2;
		ball3 = ball3 / 2;
		ball4 = ball4 / 2;

		int split1 = ball1 / 2;
		int split2 = ball2 / 2;
		int split3 = ball3 / 2;
		int split4 = ball4 / 2;

		//If any extra health reamins, split them in equal but give the remainder of the operation
		//to the first ball (arbitrary).
		int remainingHealth = totalHealth - ball1 - ball2 - ball3 - ball4 - (split1 * 2) - (split2 * 2) - (split3 * 2) - (split4 * 2);

		if (remainingHealth > 0) 
		{
			while(remainingHealth - 4 > 0) {
				remainingHealth -= 4;
				ball1++;
				ball2++;
				ball3++;
				ball4++;
			}
			ball1 += remainingHealth;
		}
		else if (remainingHealth < 0) 
		{
			while(remainingHealth + 4 < 0) {
				remainingHealth += 4;
				ball1--;
				ball2--;
				ball3--;
				ball4--;
			}
			ball2 -= remainingHealth;
		}

		//Initialize balls.
		Ball ball1in = new Ball(ball1, new List<int>() {split1, split1}, Random.Range(delayMin, delayMax));
		Ball ball2in = new Ball(ball2, new List<int>() {split2, split2}, Random.Range(delayMin, delayMax));
		Ball ball3in = new Ball(ball3, new List<int>() {split3, split3}, Random.Range(delayMin, delayMax));
		Ball ball4in = new Ball(ball4, new List<int>() {split4, split4}, Random.Range(delayMin, delayMax));

		//Create a new level with the newly randomized balls. Add the level to the levels list.
		Level newLevel = new Level(new List<Ball>() {ball1in, ball2in, ball3in, ball4in});
		levels.Add(newLevel);
		lastTotalHealth = (int) (lastTotalHealth * proceduralIncrementer);
	}

	//Canvas display functions are below.
	void DisplayWinScreen(bool on) 
	{
		winText.text = "Level " + currentLevel + " cleared!";
		winDisp.SetActive(on);
	}

	public void DisplayLoseScreen() 
	{
		scoreText.text = "Total Damage Done = " + totalDamage;
		loseText.text = "Lost at level " + currentLevel;
		Time.timeScale = 0;
		gameEnded = true;
		loseDisp.SetActive(true);
	}

	void DisplayScore() 
	{
		scoreDisp.SetActive(true);
	}

	public void RestartGame() 
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
