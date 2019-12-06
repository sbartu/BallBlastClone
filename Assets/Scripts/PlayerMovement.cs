﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Althought this script is name PlayerMovement, Shooting is also described here.
//Sorry for the slight confusion.
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigid; 
	private Vector3 moveDirection = Vector3.zero;
    private bool canShoot = true;
    public float speed;
    private float shootDelay;
    private IEnumerator coroutine;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        shootDelay = Stats.sharedInstance.shootDelay;
    }

    void FixedUpdate()
    {
        //Get inputs for movement from the Horizontal axis. Move with velocity for smoother movement.
		Vector3 velocity = (transform.right * Input.GetAxis("Horizontal")) * speed * Time.fixedDeltaTime;
		rigid.velocity = velocity;

        //Shoot if there is no shoot delay coroutine in progress.
        if (Input.GetMouseButton(0)) {
            if (canShoot) {
                Shoot();
                coroutine = WaitToShoot(shootDelay);
                StartCoroutine(coroutine);
            }

            canShoot = false;
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if(collision.gameObject.tag == "Enemy")
            LevelProgress.sharedInstance.DisplayLoseScreen();
    }

    void Shoot() {
        //Get the bullets  from the object pool and activate it
        //above the player.
        List<GameObject> bullets = new List<GameObject>();
        int bulletCount = Stats.sharedInstance.bulletCount;

        for(int i = 0; i < bulletCount; i++) 
        {
            GameObject bullet = ObjectPooler.sharedInstance.GetPooledObject("Ammo");
            bullet.SetActive(true);
            bullets.Add(bullet);
        }

        //If there is only one bullet count (1st level) just place it above the player.
        if(bulletCount == 1) 
        {
            bullets[0].transform.position = transform.position + Vector3.up;
            return;
        }

        //The following logic is for spacing the bullets above the player
        //in an ordinarily way. 
        int midpoint = bulletCount / 2;
        float offset = Stats.sharedInstance.bulletWidth / (float) bulletCount;
        float start = midpoint * offset;
        int index = midpoint;

        for(int i = 0; i < midpoint; i++) 
        {
            bullets[i].transform.position = transform.position + Vector3.up + (index * offset * Vector3.left);
            index--;
        }

        //If bullet count is odd, place one bullet right above the player.
        //Else continue spacing them.
        if(bulletCount % 2 == 0)
            index = 1;
        else
            index = 0;

        for(int i = midpoint; i < bulletCount; i++) 
        {
            bullets[i].transform.position = transform.position + Vector3.up + (index * offset * Vector3.right);
            index++;
        }
    }

    private IEnumerator WaitToShoot(float waitTime)
    {
        //The delay before next shot is allowed.
        yield return new WaitForSeconds(waitTime);
        canShoot = true;
    }
}
