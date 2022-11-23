using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Subway : MonoBehaviour {

	SubwayCar[] cars;
	Animator animator;

	TransitionManager transitionManager;

	void Start() {
		transitionManager = FindObjectOfType<TransitionManager>();
		cars = GetComponentsInChildren<SubwayCar>();
		animator = GetComponent<Animator>();
	}

	public void OnPlayerStepIn() {
		if (transitionManager.transition.subway) {
			// Arrive();
			// clear transition
		} else {
			animator.SetTrigger("Arrive");
		}
	}
}
