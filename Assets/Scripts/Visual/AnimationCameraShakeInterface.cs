using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationCameraShakeInterface : MonoBehaviour {
	CameraShake shake;

	void Awake() {
		shake = GameObject.FindObjectOfType<CameraShake>();
	}

	public void ShakeX(float force) {
		shake.Shake(new Vector2(force, 0));
	}
}
