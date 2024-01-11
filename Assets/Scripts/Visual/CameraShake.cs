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

	// TODO: for 2D shake that's a spiral, need to look at this in the player virtual camera
	// https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/manual/CinemachineVirtualCameraNoise.html

	void Awake() {
		source = GetComponent<CinemachineImpulseSource>();
	}

	public void Shake(Vector2 force) {
		if (GameOptions.ReduceCameraShake) return;
		source.GenerateImpulse(force);
	}

	public void XSmallShake() {
		Shake(xsmall);
	}

	public void SmallShake() {
		Shake(small);
	}
}
