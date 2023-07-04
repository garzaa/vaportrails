using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BeaconWrapper))]
public class Door : Interactable {
	public override void OnInteract(EntityController player) {
		FindObjectOfType<TransitionManager>().BeaconTransition(GetComponent<BeaconWrapper>().GetBeacon);
	}
}
