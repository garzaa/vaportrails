using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValCameraPoint : MonoBehaviour {
	// just raise it when she's on the ground
	GroundData groundData;
	Vector2 currentVelocity;

	public float groundOffset;
	float cameraRange = 2f;
	float cameraSmoothTime = 0.5f;
	PlayerInput input;
	Entity player;

	Vector2 targetPos;

	void Start() {
		groundData = GetComponentInParent<GroundCheck>().groundData;
		input = GetComponentInParent<PlayerInput>();
		player = input.GetComponent<Entity>();
	}

	// void Update() {
	// 	float yPos = groundData.grounded ? groundOffset : 0;

	// 	transform.localPosition = Vector3.MoveTowards(
	// 		transform.localPosition,
	// 		Vector3.up * yPos,
	// 		1f * Time.deltaTime
	// 	);
	// }

	void Update() {
		targetPos.x = input.GetAxis(RewiredConsts.Action.CameraHorizontal) * transform.lossyScale.x;
		targetPos.y = input.GetAxis(RewiredConsts.Action.CameraVertical);

		if (player.inCutscene) targetPos = Vector2.zero;

		targetPos *= cameraRange;
		transform.localPosition = Vector2.SmoothDamp(
			transform.localPosition,
			targetPos,
			ref currentVelocity,
			cameraSmoothTime
		);
	}
}
