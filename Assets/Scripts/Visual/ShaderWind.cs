using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ShaderWind : MonoBehaviour, IWindReceiver {
	Vector4 baseUV = Vector2.zero;
	Material material;

	// for instanced materials that animations are grabbing
	Renderer r;
	MaterialPropertyBlock block;

	float windSpeed = 0;
	float windSize = 0;
	float windStrength = 0;
	float windDir = 0;

	Vector3 posLastFrame = Vector3.zero;

	void Start() {
		r = GetComponent<Renderer>();
		material = r.sharedMaterial;
		
		block = new MaterialPropertyBlock();
		r.GetPropertyBlock(block);
		posLastFrame = transform.position;
	}

	void Update() {
		// this exists to stop clouds jumping around when wind speed is changed
		// shaders can't persiste vectors between frames
		// wait but also compare it to the actual object's position last frame
		baseUV += (Vector4) Vector2.right * (windSpeed * Time.deltaTime) * windDir;

		// it does move around to follow the player camera so we need to keep track of that
		// does this need to be multiplied with the PPU or something...dios mio
		baseUV -= (Vector4) (transform.position - posLastFrame) * 7;
		material.SetVector("_BaseUV", baseUV);

		if (block != null && !block.isEmpty) {
			block.SetVector("_BaseUV", baseUV);
			r.SetPropertyBlock(block);
		}

		posLastFrame = transform.position;
	}

	public void Wind(float speed, float size, float strength, float dir) {
		// set the base UV
		this.windSpeed = speed;
		this.windSize = size;
		this.windStrength = strength;
		this.windDir = dir;
	}
}
