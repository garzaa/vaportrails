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

	// called as an event from player trigger
	public void OnPlayerStepIn() {
		if (transitionManager.transition.subway) {
			Arrive();
			// clear transition
		} else {
			animator.SetTrigger("Arrive");
		}
	}

	void Arrive() {
		// move player to relative position
		// hide the player
		// move the player dummy to relative position
	}

	public void OnPlayerStepOut() {
		// depart, either with player or without
	}
}
