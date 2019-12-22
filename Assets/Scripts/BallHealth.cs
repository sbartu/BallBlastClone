using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallHealth : MonoBehaviour {

	public int health = 10;
	public List<int> splits;
	public float delay;
	public float splitForceOffset = 0.01f;
	public float colorChangeFrequency = 0.1f;

	private bool left;
	private int colorsLength;
	private float colorIndex = 0f;
	private float newT;
	private float newFreq;
	private Color[] colors = {Color.green, Color.cyan, Color.magenta, Color.yellow};
	private Color oldColor;
	private Color newColor;
	private Renderer rend;
	private Vector3 explodePosition;
	private Text healthText;
	private Rigidbody rigid;


	void Start () 
	{
		colorsLength = colors.Length;
		rend = GetComponent<Renderer>();
		rigid = GetComponent<Rigidbody>();
		newFreq = colorChangeFrequency;
		left = true;
	}

	void OnEnable () 
	{
		healthText = GetComponentInChildren<Text>();
		UpdateHealthText();
	}

	void UpdateHealthText () 
	{
		healthText.text = health.ToString();
	}

	void OnTriggerEnter (Collider other) 
	{
		//When hit by a bullet reduce health by the necessary amount
		//and change color of the ball. Also deactivate that bullet.
		if(other.gameObject.tag == "Ammo") 
		{
			int damage = Stats.sharedInstance.bulletDamage;
			int currentHealth = health;
			currentHealth -= damage;

			if(currentHealth < 0)
				currentHealth = 0;

			//Add up the score based on lost health.
			int healthDiff = health - currentHealth;
			LevelProgress.sharedInstance.currentDamage += healthDiff;
			LevelProgress.sharedInstance.totalDamage += healthDiff;
			
			health = currentHealth;
			other.gameObject.SetActive(false);
			ChangeColor();
			UpdateHealthText();
			LevelProgress.sharedInstance.UpdateScoreText();

			if(health <= 0) 
			{
				//When health is depleted, move to outside of gamefield and setActive false.
				explodePosition = transform.position;
				transform.position = new Vector3(10f, 0f, 0f);
				
				//Decreases number of enemies left by 1. Ends level if it reaches 0.
				LevelProgress.sharedInstance.DecreaseBallNum();

				if(splits != null) 
				{
					Vector3 newSize = GetHalfSize();
					foreach(int hp in splits) 
					{
						SpawnSplit(hp, explodePosition, newSize, left);
						left = !left;
					}
				}
				rigid.velocity = Vector3.zero;
				rigid.angularVelocity = Vector3.zero;
				gameObject.SetActive(false);

				//Shake Camera for flavor.
				Camera.main.GetComponent<ShakeCamera>().ShakeCam();
			}
		}
	}
	
	void SpawnSplit (int hp, Vector3 explosionPos, Vector3 newSize, bool left) 
	{
		//Grab new enemies from the pool to be used as the splits.
		GameObject enemy = ObjectPooler.sharedInstance.GetPooledObject("Enemy"); 
        if (enemy != null) {
			BallHealth enemyHealth = enemy.GetComponent<BallHealth>();
			Rigidbody rigid = enemy.GetComponent<Rigidbody>();

            enemy.transform.position = explosionPos;
			enemy.transform.localScale = newSize;
			enemyHealth.health = hp;
			enemyHealth.splits = null;
            enemy.SetActive(true);

			//On Explosion, add an up force to both splits. Add a left force to one
			//and a right force to the other.
			Vector3 splitForce = (left ? Vector3.left + Vector3.up : Vector3.right + Vector3.up) * splitForceOffset;
			rigid.AddRelativeForce(splitForce);
        }
	}

	void ChangeColor () 
	{
		//Lerp between some colors as damage is received for flavor.
		oldColor = colors[(int) colorIndex % colorsLength];
		newColor = colors[(int) (colorIndex + 1f) % colorsLength];;
		rend.material.color = Color.Lerp(oldColor, newColor, newFreq);

		newFreq += colorChangeFrequency;
		newFreq %= 1;

		colorIndex += colorChangeFrequency;
	}

	Vector3 GetHalfSize() 
	{
		//Halve the size of the splits compared to the original ball.
		float halfSize = transform.localScale.x / 2;
		return new Vector3(halfSize, halfSize, 0.25f);
	}
}
