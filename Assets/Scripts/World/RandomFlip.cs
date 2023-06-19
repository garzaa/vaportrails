using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomFlip : MonoBehaviour {
	void Start() {
		transform.localScale = new Vector3(
			Random.value > 0.5 ? 1 : -1,
			1,
			1
		);
	}
}
