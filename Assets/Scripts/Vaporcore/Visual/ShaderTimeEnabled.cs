using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaderTimeEnabled : MonoBehaviour {
	Renderer renderer = null;
	MaterialPropertyBlock material;
    
	void OnEnable() {
		if (renderer == null) {
			renderer = GetComponent<Renderer>();
			renderer.GetPropertyBlock(material);
		}
		renderer.SetFloat("TimeEnabled", Time.time);
	}

	void OnDestroy() {
		Destroy(renderer.material);
	}
}
