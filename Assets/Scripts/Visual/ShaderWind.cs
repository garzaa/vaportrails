using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ShaderWind : MonoBehaviour, IWindReceiver {
	Vector4 baseUV = Vector2.zero;
	Material material;

	float windSpeed = 0;
	float windSize = 0;
	float windStrength = 0;
	float windDir = 0;

	void Start() {
		material = GetComponent<Renderer>().sharedMaterial;
	}

	void Update() {
		// this exists to stop clouds jumping around when wind speed is changed
		// shaders can't persiste vectors between frames
		baseUV += (Vector4) Vector2.right * (windSpeed * Time.deltaTime) * windDir;
		material.SetVector("_BaseUV", baseUV);
	}

	public void Wind(float speed, float size, float strength, float dir) {
		// set the base UV
		this.windSpeed = speed;
		this.windSize = size;
		this.windStrength = strength;
		this.windDir = dir;
	}
}
