using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RandomObject : MonoBehaviour {
	public GameObject[] candidates;

	void Start() {
		// sometimes it's not getting rotated if it's a tilemap that starts after this
		GameObject g = Instantiate(
			candidates[Random.Range(0, candidates.Length)],
			transform.position,
			transform.rotation,
			transform
		);
		StartCoroutine(SetNextFrameRotation(g));
	}

	IEnumerator SetNextFrameRotation(GameObject g) {
		yield return new WaitForEndOfFrame();
		g.transform.rotation = transform.rotation;
	}
}
