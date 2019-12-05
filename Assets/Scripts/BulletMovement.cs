using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour {

	private Rigidbody rigid; 
	private Vector3 moveDirection = Vector3.zero;
    public float speed;
	public GameObject player;

	// Use this for initialization
	void OnEnable () 
	{
		rigid = GetComponent<Rigidbody>();
		SetSpeed();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void SetSpeed () 
	{
		Vector3 velocity = (transform.up * speed * Time.fixedDeltaTime);
		rigid.velocity = velocity;
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag == "TopWall")  {
			gameObject.SetActive(false);
		}
	}
}
