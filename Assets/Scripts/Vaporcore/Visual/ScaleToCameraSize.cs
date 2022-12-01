using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ScaleToCameraSize : MonoBehaviour {

	Camera cam;
	Vector3 scale;

	public bool x = true;
	public bool y = true;

	void Start() {
		cam = Camera.main;
		scale = transform.localScale;
	}

	void Update() {
		scale = transform.localScale;
		if (y) scale.y = 2f*cam.orthographicSize;
		if (x) scale.x = 2f*cam.orthographicSize*cam.aspect;
		transform.localScale = scale;
	}
}
