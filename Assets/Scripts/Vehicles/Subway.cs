using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Subway : MonoBehaviour {

	public enum SubwayDirection {
		LEFT = 0,
		RIGHT = 1,
	}

	public string stopDestination;
	public string lineName;
	public Text infoText;

	public SubwayDirection leaveDirection;
	public SubwayDirection arriveDirection;

	public GameObject playerDummy;
	GameObject player;

	SubwayCar[] cars;
	Animator animator;

	bool holdingPlayer;
	bool doorsOpen;
	PlayerInput playerOne;

	int cycleLength = 10 + 10 + 10 + 10;

	void Start() {
		cars = GetComponentsInChildren<SubwayCar>();
		animator = GetComponent<Animator>();
		player = PlayerInput.GetPlayerOneInput().gameObject;
		playerDummy.SetActive(false);
		animator.SetFloat("LeaveDirection", (float) leaveDirection);
		animator.SetFloat("ArriveDirection", (float) arriveDirection);
		StartCoroutine(SubwayRoutine());
		foreach (SubwayCar car in cars) {
			car.DisableBoarding();
		}
		playerOne = PlayerInput.GetPlayerOneInput();
	}

	void Update() {
		if (holdingPlayer && doorsOpen && playerOne.VerticalInput() < -0.3f) {
			OffboardPlayer();
		}
	}

	IEnumerator SubwayRoutine() {
		for (;;) {
			ArriveAtStation();
			SetInfoText(NextStopTime(0));
			// arrive time + wait at station time
			yield return new WaitForSeconds(10);
			LeaveStation();
			SetInfoText(NextStopTime(30));
			yield return new WaitForSeconds(10);
			SetInfoText(NextStopTime(20));
			yield return new WaitForSeconds(10);
			SetInfoText(NextStopTime(10));
			yield return new WaitForSeconds(10);
		}
	}

	void SetInfoText(string s) {
		StartCoroutine(UpdateBoard(s.Split('\n')));
	}

	IEnumerator UpdateBoard(string[] s) {
		infoText.text = s[0];
		yield return new WaitForSeconds(0.2f);
		infoText.text = s[0] + "\n" + s[1];
		yield return new WaitForSeconds(0.2f);
		infoText.text = s[0] + "\n" + s[1] + "\n" + s[2];
	}

	string NextStopTime(int time) {
		string top = "";
		if (time == 0) top = RouteInfo() + " now arriving";
		else top = RouteInfo() + ": " + time+"s";
		string middle = RouteInfo() + ": " + FormatSeconds(time+cycleLength);
		string bottom = RouteInfo() + ": " + FormatSeconds(time+cycleLength+cycleLength);
		return top + "\n" + middle + "\n" + bottom;
	}

	string RouteInfo() {
		return $"{lineName} line to {stopDestination}";
	}

	string FormatSeconds(int seconds) {
		if (seconds < 60) return seconds+"s";
		return (seconds / 60)+"m " + (seconds % 60)+"s"; 
	}

	void ArriveAtStation() {
		if (holdingPlayer) {
			if (arriveDirection == SubwayDirection.RIGHT) {
				playerDummy.transform.localScale = new Vector3(-1, 1, 1);
			} else {
				playerDummy.transform.localScale = new Vector3(-1, 1, 1);
			}
		}
		animator.SetTrigger("Arrive");
	}

	public void LeaveStation() {
		animator.SetTrigger("Depart");
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
		holdingPlayer = true;
	}

	public void FinishDepartAnimation() {
		if (holdingPlayer) {
			// set the subway transition and DO IT
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

		// face towards subway leave direction
		Entity playerEntity = player.GetComponent<Entity>();
		if (leaveDirection == SubwayDirection.RIGHT) {
			if (!playerEntity.facingRight) playerEntity.Flip();
			playerDummy.transform.localScale = new Vector3(-1, 1, 1);
		} else {
			if (playerEntity.facingRight) playerEntity.Flip();
			playerDummy.transform.localScale = new Vector3(1, 1, 1);
		}
		// show player dummy
		playerDummy.SetActive(true);
		holdingPlayer = true;
	}

	void OffboardPlayer() {
		player.SetActive(true);
		playerDummy.SetActive(false);
	}

	// called from arriving animation
	public void OpenAllDoors() {
		doorsOpen = true;
		foreach (SubwayCar car in cars) {
			car.OpenDoors();
			car.EnableBoarding();
		}
	}

	// called from departing animation
	public void CloseAllDoors() {
		doorsOpen = false;
		foreach (SubwayCar car in cars) {
			car.CloseDoors();
			car.DisableBoarding();
		}
	}
}