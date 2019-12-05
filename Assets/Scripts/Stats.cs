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
		//Store the input created from the json file.
		input = InputData.CreateFromJSON();
		sharedInstance = this;
	}

	void Start () 
	{
		//Initialize stats from the given json.
		gravity = input.gravity;
		bulletCountIncrease = input.bullet_count_increase;
		bulletDamageIncrease = input.bullet_damage_increase;
	}

	public void IncreaseStats () 
	{
		//Increase stats by given amount from the json.
		bulletCount += bulletCountIncrease;
		bulletDamage += bulletDamageIncrease;
	}

	public Vector3 GetRandomSize () 
	{
		//Randomize ball sizes for flavor.
		float newSize = Random.Range(ballMinSize, ballMaxSize);
		return new Vector3(newSize, newSize, 0.25f);
	}
}
