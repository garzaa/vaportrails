using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RandomObject : MonoBehaviour {
	public GameObject[] candidates;

	void Start() {
		GameObject g = Instantiate(
			candidates[Random.Range(0, candidates.Length)],
			transform.position,
			transform.rotation,
			transform
		);
	}
}
