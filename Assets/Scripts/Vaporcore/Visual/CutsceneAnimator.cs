using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneAnimator : MonoBehaviour {
#if UNITY_EDITOR
	void Update() {
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			Time.timeScale = 4;
		} else if (Input.GetKeyUp(KeyCode.RightArrow)) {
			Time.timeScale = 1;
		}
	}
#endif
}
