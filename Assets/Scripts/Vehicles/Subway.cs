using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Subway : MonoBehaviour {

	public GameObject playerDummy;
	GameObject player;

	SubwayCar[] cars;
	Animator animator;

	bool arriving = false;
	bool playerInStation = false;

	void Start() {
		cars = GetComponentsInChildren<SubwayCar>();
		animator = GetComponent<Animator>();
		player = PlayerInput.GetPlayerOneInput().gameObject;
		playerDummy.SetActive(false);
	}

	// called as an event from player trigger
	public void OnPlayerEnterTrigger() {
		playerInStation = true;
		ArriveAtStation();
	}

	void ArriveAtStation() {
		animator.SetTrigger("Arrive");
		arriving = true;
	}	

	public void OnPlayerLeaveTrigger() {
		if (arriving) return;
		playerInStation = false;
		LeaveStation();
	}

	public void LeaveStation() {
		// called if either player steps out of volume or transitionmanager says to leave
		animator.SetTrigger("Depart");
		animator.ResetTrigger("Arrive");
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
		arriving = false;
		foreach (SubwayCar car in cars) {
			car.OpenDoors();
		}
		if (!playerInStation) {
			LeaveStation();
		}
		player.SetActive(true);
		playerDummy.SetActive(false);
	}

	public void FinishDepartAnimation() {
		if (playerInStation) {
			ArriveAtStation();
		}
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
