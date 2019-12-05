using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {

	public static Stats sharedInstance;
	public float gravity;
	public int bulletCountIncrease;
	public int bulletDamageIncrease;
	public int bulletCount = 1;
	public int bulletDamage = 1;
	public float shootDelay;
	public InputData input;
	public float ballMaxSize = 2.75f;
	public float ballMinSize = 0.75f;

	void Awake() 
	{
		input = InputData.CreateFromJSON();
		sharedInstance = this;
	}

	void Start () 
	{
		gravity = input.gravity;
		bulletCountIncrease = input.bullet_count_increase;
		bulletDamageIncrease = input.bullet_damage_increase;
	}

	public void IncreaseStats () 
	{
		bulletCount += bulletCountIncrease;
		bulletDamage += bulletDamageIncrease;
	}

	public Vector3 GetRandomSize () 
	{
		float newSize = Random.Range(ballMinSize, ballMaxSize);
		Debug.Log(newSize);
		return new Vector3(newSize, newSize, 0.25f);
	}
}
