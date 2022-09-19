using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ScaleToCameraSize : MonoBehaviour {

	Camera cam;

	void Start() {
		cam = Camera.main;
	}

	void Update() {
		transform.localScale = new Vector2(2f*cam.orthographicSize*cam.aspect, 2f*cam.orthographicSize);
	}
}
