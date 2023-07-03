using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneLoadTrigger : PlayerTriggeredObject {
	public BeaconWrapper beacon;

	protected override void OnPlayerEnter(Collider2D player) {
		FindObjectOfType<TransitionManager>().BeaconTransition(beacon.GetBeacon);
	}
}
