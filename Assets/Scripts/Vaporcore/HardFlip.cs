using UnityEngine;

public class HardFlip : MonoBehaviour {
	void LateUpdate() {
		Vector3 s = transform.localScale;
		if (s.x > 0 && s.x < 1) {
			s.x = 1;
		} else if (s.x < 0 && s.x > -1) {
			s.x = -1;
		}
		transform.localScale = s;
	}
}
