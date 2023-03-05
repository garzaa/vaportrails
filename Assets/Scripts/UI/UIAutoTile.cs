using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIAutoTile : MonoBehaviour {
	RectTransform rectTransform;
	Image image;
	Material m;

	void Start() {
		rectTransform = GetComponent<RectTransform>();
		image = GetComponent<Image>();
	}

	void Update() {
		m = image.canvasRenderer.GetMaterial(0);
		if (m == null || rectTransform == null) return;
		m.mainTextureScale = rectTransform.rect.size / image.sprite.rect.size;
		// m.mainTextureScale = Vector2.one;
		image.canvasRenderer.SetMaterial(m, 0);
	}
}
