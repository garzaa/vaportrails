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

	StandaloneMap quickMap;

	void Start() {
		groundData = GetComponentInParent<GroundCheck>().groundData;
		input = GetComponentInParent<PlayerInput>();
		player = input.GetComponent<Entity>();
		quickMap = FindObjectOfType<StandaloneMap>(includeInactive: true);
	}

	void Update() {
		targetPos.x = input.GetAxis(RewiredConsts.Action.CameraHorizontal) * transform.lossyScale.x;
		targetPos.y = input.GetAxis(RewiredConsts.Action.CameraVertical);

		if (player.inCutscene || ((quickMap?.gameObject.activeInHierarchy ?? false) && quickMap.open)) {
			targetPos = Vector2.zero;
		}

		targetPos *= cameraRange;
		targetPos *= GameOptions.Lookahead;
		transform.localPosition = Vector2.SmoothDamp(
			transform.localPosition,
			targetPos,
			ref currentVelocity,
			cameraSmoothTime
		);
	}
}
