using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValCameraPoint : MonoBehaviour {
	// just raise it when she's on the ground
	GroundData groundData;
	Vector3 currentVelocity;

	public float groundOffset;

	void Start() {
		groundData = GetComponentInParent<GroundCheck>().groundData;
	}

	// void Update() {
	// 	float yPos = groundData.grounded ? groundOffset : 0;

	// 	transform.localPosition = Vector3.MoveTowards(
	// 		transform.localPosition,
	// 		Vector3.up * yPos,
	// 		1f * Time.deltaTime
	// 	);
	// }
}
