using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraStopper : PlayerTriggeredObject {
	CameraInterface cameraInterface;
	public GameObject optionalTarget;

	protected override void Start() {
		base.Start();
		cameraInterface = FindObjectOfType<CameraInterface>();
	}

	override protected void OnPlayerEnter(Collider2D player) {
		if (optionalTarget) cameraInterface.SetMainTarget(optionalTarget);
		else cameraInterface.StopFollowingPlayer();
	}

	override protected void OnPlayerExit(Collider2D player) {
		cameraInterface.ResetMainTarget();
	}
}
