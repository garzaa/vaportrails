using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCameraControls : MonoBehaviour {
	public GameObject cameraContainer;
	public float moveSpeed = 15f;
	Vector3 nav = Vector3.zero;
	PlayerInput input;
	PolygonCollider2D cameraBounds;
	float z;

	void Start() {
		input = PlayerInput.GetPlayerOneInput();
		cameraBounds = GameObject.Find("MapCameraBounds").GetComponent<PolygonCollider2D>();
		z = cameraContainer.transform.position.z;
	}

	void Update() {
		nav.x = input.GetAxis(RewiredConsts.Action.CameraHorizontal);
		nav.y = input.GetAxis(RewiredConsts.Action.CameraVertical);
		cameraContainer.transform.localPosition += nav * moveSpeed * Time.deltaTime;

		cameraContainer.transform.position = cameraBounds.ClosestPoint(cameraContainer.transform.position);
		nav = cameraContainer.transform.position;
		nav.z = z;
		cameraContainer.transform.position = nav;
	}

	void OnDisable() {
		if (cameraContainer) cameraContainer.transform.localPosition = Vector3.zero;
	}
}
