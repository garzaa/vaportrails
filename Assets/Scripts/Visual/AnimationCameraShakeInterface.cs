using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationCameraShakeInterface : MonoBehaviour {
	CameraShake shake;

	void Awake() {
		shake = GameObject.FindObjectOfType<CameraShake>();
	}

	public void ShakeX(float force) {
		shake.Shake(Vector2.right * force);
	}

	public void ShakeY(float force) {
		shake.Shake(Vector2.up * force);
	}
}
