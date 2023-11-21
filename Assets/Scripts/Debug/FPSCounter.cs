using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {
	Text counterText;

	void Start() {
		counterText = GetComponentInChildren<Text>();
	}

	void Update() {
		counterText.text = ((int) (1f/Time.deltaTime) * Time.timeScale) + " fps";
	}
}
