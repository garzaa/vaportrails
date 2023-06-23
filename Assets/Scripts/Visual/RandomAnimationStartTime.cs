using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomAnimationStartTime : MonoBehaviour {
	void Start() {
		GetComponent<Animator>().Update(Random.Range(0, 20));
	}
}

