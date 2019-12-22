using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour {

	private Rigidbody rigid; 
	private Vector3 moveDirection = Vector3.zero;
    public float speed;

	void OnEnable () 
	{
		//Since we are using object pooling. The velocity is set for the bullet OnEnable.
		rigid = GetComponent<Rigidbody>();
		SetSpeed();
	}

	public void SetSpeed () 
	{
		//Propel the bullet upwards with a fixed velocity.
		Vector3 velocity = (transform.up * speed * Time.fixedDeltaTime);
		rigid.velocity = velocity;
	}

	void OnTriggerEnter (Collider other)
	{
		//Deactivate the bullet when it hits the ceiling.
		if (other.gameObject.tag == "TopWall")  {
			gameObject.SetActive(false);
		}
	}
}
