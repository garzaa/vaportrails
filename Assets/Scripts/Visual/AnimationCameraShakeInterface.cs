using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationCameraShakeInterface : MonoBehaviour {
	CameraShake shake;

	public void Shake(Vector2 force) {
		shake.CustomShake(force);
	}
}
