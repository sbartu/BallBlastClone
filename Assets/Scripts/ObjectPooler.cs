using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Serializable object pools to be used by
//bullets and balls.
[System.Serializable]
public class ObjectPoolItem {
	public int amountToPool;
	public GameObject objectToPool;
	public bool shouldExpand;
}

public class ObjectPooler : MonoBehaviour {

	public static ObjectPooler sharedInstance;
	public List<ObjectPoolItem> itemsToPool;
	public List<GameObject> pooledObjects;

	void Awake() 
	{
		//Create a shared instance to be used by other scripts.
		sharedInstance = this;
	}

	void Start () 
	{
		//Instantiate objects into their respectives pools
		//for the given amount.
		pooledObjects = new List<GameObject>();
		foreach (ObjectPoolItem item in itemsToPool) {
			for (int i = 0; i < item.amountToPool; i++) {
				GameObject obj = (GameObject)Instantiate(item.objectToPool);
				obj.SetActive(false);
				pooledObjects.Add(obj);
			}
		}
	}

	public GameObject GetPooledObject(string tag) {
		//Return non-active pool object by tag.
		for (int i = 0; i < pooledObjects.Count; i++) {
			if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].tag == tag) {
				return pooledObjects[i];
			}
		}
		//If all the objects are active, insantiate a new one if the pools it ok to be expanded.
		foreach (ObjectPoolItem item in itemsToPool) {
			if (item.objectToPool.tag == tag) {
				if (item.shouldExpand) {
					GameObject obj = (GameObject)Instantiate(item.objectToPool);
					obj.SetActive(false);
					pooledObjects.Add(obj);
					return obj;
				}
			}
		}
		//Otherwise return null.
		return null;
	}
}
