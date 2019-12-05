using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
		Vector3 velocity = (transform.right * Input.GetAxis("Horizontal")) * speed * Time.fixedDeltaTime;
		rigid.velocity = velocity;

        if (Input.GetMouseButton(0)) {
            if (canShoot) {
                Shoot();
                coroutine = WaitToShoot(shootDelay);
                StartCoroutine(coroutine);
            }

            canShoot = false;
        }
    }

    void Shoot() {
        GameObject bullet = ObjectPooler.sharedInstance.GetPooledObject("Ammo"); 
        if (bullet != null) {
            bullet.transform.position = transform.position + Vector3.up;
            bullet.SetActive(true);
        }
    }

    private IEnumerator WaitToShoot(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        canShoot = true;
    }
}
