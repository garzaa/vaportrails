using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ParallaxUVOffset : MonoBehaviour {

	Renderer r;
	MaterialPropertyBlock block = null;

	void Start() {
		r = GetComponent<Renderer>();
		block = new MaterialPropertyBlock();
		r.GetPropertyBlock(block);
	}

	void Update() {
		if (block != null) block.SetVector("ParallaxUVOffset", transform.position);
		r.SetPropertyBlock(block);
	}
}
