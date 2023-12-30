using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabSpawner : MonoBehaviour {
	public GameObject template;

	public void SpawnAtTarget(GameObject target) {
		Instantiate(template, target.transform.position, Quaternion.identity);
	}
}
