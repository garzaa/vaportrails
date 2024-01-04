using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForceSafetyRespawnPoint : PlayerTriggeredObject {
	protected override void OnPlayerEnter(Collider2D player) {
		player.GetComponent<Entity>().ForceSafetyRespawnPoint(this.gameObject, Vector3.zero);
	}
}
