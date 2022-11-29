using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this should be on a parallax layer with speed 1
// it will dynamically resize itself

[ExecuteInEditMode]
public class PerspectiveWater : MonoBehaviour {
	// this should be a child and of size 1x1
	public GameObject waterSprite;

	// this should be on a front layer
	public GameObject frustumIntersection;

	Camera cam;

	Vector3 scale, position;

	void Start() {
		cam = Camera.main;
	}

	void Update() {
		// stretch the sprite to the bottom 
		float distance = frustumIntersection.transform.position.y - transform.position.y;
		scale = waterSprite.transform.localScale;
		scale.y = distance;

		// and widen it to the camera width if in-game
		if (Application.isPlaying) scale.x = 2*cam.orthographicSize*cam.aspect;
		else scale.x = 20;

		waterSprite.transform.localScale = scale;

		position = waterSprite.transform.position;
		position.y = transform.position.y + distance/2f;
		waterSprite.transform.position = position;
	}

}
