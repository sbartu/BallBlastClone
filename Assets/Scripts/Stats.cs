using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {

	public static Stats sharedInstance;
	public InputData input;
	public float shootDelay;
	public int bulletCount = 1;
	public int bulletDamage = 1;
	public float ballMaxSize = 2.75f;
	public float ballMinSize = 0.75f;
	public float bulletWidth = 0.5f;

	private float gravity;
	private int bulletCountIncrease;
	private int bulletDamageIncrease;

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
		//Set gravity.
		Physics.gravity = new Vector3(0, gravity, 0);
	}

	public void IncreaseStats () 
	{
		//Increase stats by given amount from the json.
		bulletCount += bulletCountIncrease;
		bulletDamage += bulletDamageIncrease;
		//Increase bullet width so multiple bullets don't stack up on top
		//of each other.
		bulletWidth += 0.1f;
	}

	public Vector3 GetRandomSize () 
	{
		//Randomize ball sizes for flavor.
		float newSize = Random.Range(ballMinSize, ballMaxSize);
		return new Vector3(newSize, newSize, 0.25f);
	}
}
