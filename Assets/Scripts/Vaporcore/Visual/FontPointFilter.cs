using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FontPointFilter : MonoBehaviour {
	public List<Font> fonts;

	void Start() {
		foreach (Font f in fonts) {
			f.material.mainTexture.filterMode = FilterMode.Point;
		}
	}
}
