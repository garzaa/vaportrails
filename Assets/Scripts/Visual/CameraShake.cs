using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class CameraShake : MonoBehaviour {
	CinemachineImpulseSource source;
	
	public Vector2 xsmall;
	public Vector2 small;
	public Vector2 medium;
	public Vector2 large;

	void Awake() {
		source = GetComponent<CinemachineImpulseSource>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.K)) {
			source.GenerateImpulse(xsmall);
		}
	}

	public void CustomShake(Vector2 force) {
		source.GenerateImpulse(force);
	}

	public void XSmallShake() {
		source.GenerateImpulse(xsmall);
	}

	public void SmallShake() {
		source.GenerateImpulse(small);
	}
}
