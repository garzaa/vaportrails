using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ShaderTimeEnabled : MonoBehaviour {
	Renderer shaderRenderer = null;
	MaterialPropertyBlock material;
	
	[SerializeField] float interval = 5;
	float timeEnabled;
    
	void OnEnable() {
		if (shaderRenderer == null) {
			shaderRenderer = GetComponent<Renderer>();
			shaderRenderer.GetPropertyBlock(material);
		}
		material.SetFloat("TimeEnabled", Time.time);
		timeEnabled = Time.time;
	}

#if UNITY_EDITOR
	void Update() {
		if (!Application.isPlaying) {
			if (Time.time > timeEnabled+interval) {
				OnEnable();
			}
		}
	}
#endif

	void OnDestroy() {
		Destroy(shaderRenderer.material);
	}
}
