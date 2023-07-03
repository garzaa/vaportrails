using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneLoadTrigger : PlayerTriggeredObject {
	public Beacon beacon;

	public void OnPlayerEnter(GameObject player) {
		FindObjectOfType<TransitionManager>().BeaconTransition(beacon);
	}
}
