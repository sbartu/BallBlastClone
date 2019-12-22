using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCamera : MonoBehaviour {

	public float shakeStart;
	public float shakeDecrement;

	private float orgZpos;
	private Vector3 orgPos, shakePos;
	private bool shaking;

	// Use this for initialization
	void Start ()
    {
        orgPos = transform.position;
        shaking = false;
		orgZpos = transform.position.z;
	}

	public void ShakeCam() 
	{
		if(!shaking)
			StartCoroutine(Shake());
	}

    IEnumerator Shake()
    {
		float decrementOffset = Time.deltaTime;
		shaking = true;
		float currentShake = shakeStart;
		while(currentShake >= 0) {
			shakePos = orgPos + (Random.insideUnitSphere * currentShake);
			shakePos.z = orgZpos;
			transform.position = shakePos;

			currentShake -= shakeDecrement * decrementOffset;
			yield return null;
		}
		shaking = false;
    }
}
