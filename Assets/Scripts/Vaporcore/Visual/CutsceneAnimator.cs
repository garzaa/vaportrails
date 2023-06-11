using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneAnimator : MonoBehaviour {
	Animator animator;

	void Start() {
		animator = GetComponent<Animator>();
	}

	void Update() {
		if (Input.GetKey(KeyCode.RightArrow)) {
			animator.speed = 4;
		} else {
			animator.speed = 1;
		}
	}
}
