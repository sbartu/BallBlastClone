using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallHealth : MonoBehaviour {

	public int health;
	public List<int> splits;
	public float delay;
	private bool left = true;
	public float offset = 0.2f;
	public Color[] colors = {Color.green, Color.cyan, Color.magenta, Color.yellow};
	private int colorsLength;
	public float colorChangeFrequency = 0.1f;

	private float colorIndex = 0f;
	private Color oldColor;
	private Color newColor;
	private float newT;
	private Renderer rend;
	private float newFreq;
	private Vector3 explodePosition;
	private Text healthText;


	void Start () 
	{
		colorsLength = colors.Length;
		rend = GetComponent<Renderer>();
		healthText = GetComponentInChildren<Text>();
		newFreq = colorChangeFrequency;
	}

	void Update () 
	{
		healthText.text = health.ToString();
		if(health <= 0) 
		{
			//When health is depleted, move to outside of gamefield and setActive false.
			explodePosition = transform.position;
			transform.position = new Vector3(10f, 0f, 0f);
			gameObject.SetActive(false);
			if(splits != null) 
			{
				Vector3 newSize = GetHalfSize();
				foreach(int hp in splits) 
				{
					SpawnSplit(hp, explodePosition, newSize, left);
					left = !left;
				}
			}
		}
	}

	void OnTriggerEnter (Collider other) 
	{
		//When hit by a bullet reduce health by the necessary amount
		//and change color of the ball. Also deactivate that bullet.
		if(other.gameObject.tag == "Ammo") 
		{
			health -= Stats.sharedInstance.bulletDamage;
			other.gameObject.SetActive(false);
			ChangeColor();
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
			rigid.AddForce((left ? Vector3.left : Vector3.right) * offset);
			rigid.AddForce(Vector3.up * offset);
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
