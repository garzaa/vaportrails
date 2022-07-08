using UnityEngine;

public class HardFlip : MonoBehaviour {
	Vector3 s;

	void LateUpdate() {
		s = transform.localScale;
		if (s.x > 0 && s.x < 1) {
			s.x = 1;
		} else if (s.x < 0 && s.x > -1) {
			s.x = -1;
		}
		transform.localScale = s;
	}
}
