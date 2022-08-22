using UnityEngine;

public class NoMirror : MonoBehaviour {
	Vector3 s;

	void LateUpdate() {
		if (transform.lossyScale.x < 0) {
			s = transform.localScale;
			s.x *= -1;
			transform.localScale = s;
		}
	}
}
