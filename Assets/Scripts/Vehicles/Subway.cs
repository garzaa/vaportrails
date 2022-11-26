using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Subway : MonoBehaviour {

	public GameObject playerDummy;
	GameObject player;

	SubwayCar[] cars;
	Animator animator;

	bool atStation = false;

	void Start() {
		cars = GetComponentsInChildren<SubwayCar>();
		animator = GetComponent<Animator>();
		player = PlayerInput.GetPlayerOneInput().gameObject;
		playerDummy.SetActive(false);
	}

	// called as an event from player trigger
	public void OnPlayerEnterTrigger() {
		if (atStation) {
			return;
		}
		ArriveAtStation();
	}

	void ArriveAtStation() {
		animator.SetTrigger("Arrive");
		atStation = true;
	}

	public void OnPlayerLeaveTrigger() {
		// called if either player steps out of volume or 
		LeaveStation();
	}

	void LeaveStation() {
		atStation = false;
		animator.SetTrigger("Leave");
	}

	public void LoadRidingPlayer(Transition.SubwayTransition subwayTransition) {
		// move player to relative position
		player.transform.position = transform.position + Vector3.right*subwayTransition.xOffset;
		// hide the player
		player.SetActive(false);

		// move the player dummy to relative position
		playerDummy.transform.localPosition = new Vector3(
			subwayTransition.xOffset,
			playerDummy.transform.localPosition.y,
			playerDummy.transform.localPosition.z
		);
		// show player dummy
		playerDummy.SetActive(true);
	}

	public void FinishArriveAnimation() {
		foreach (SubwayCar car in cars) {
			car.OpenDoors();
		}
		player.SetActive(true);
		playerDummy.SetActive(false);
	}

	// called from interaction
	public void BoardPlayer() {
		// hide player
		player.SetActive(false);
		// save the x offset to the transition
		float xOffset = player.transform.position.x - transform.position.x;
		// move player dummy to player point x
		playerDummy.transform.localPosition = new Vector3(
			xOffset,
			playerDummy.transform.localPosition.y,
			playerDummy.transform.localPosition.z
		);
		// show player dummy
		playerDummy.SetActive(true);
	}

	// called from departing animation
	public void CloseAllDoors() {
		foreach (SubwayCar car in cars) {
			car.CloseDoors();
		}
	}
}
