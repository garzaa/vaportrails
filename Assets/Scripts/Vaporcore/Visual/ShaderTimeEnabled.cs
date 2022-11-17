using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaderTimeEnabled : MonoBehaviour {
	Renderer shaderRenderer = null;
	MaterialPropertyBlock material;
    
	void OnEnable() {
		if (shaderRenderer == null) {
			shaderRenderer = GetComponent<Renderer>();
			shaderRenderer.GetPropertyBlock(material);
		}
		material.SetFloat("TimeEnabled", Time.time);
	}

	void OnDestroy() {
		Destroy(shaderRenderer.material);
	}
}
