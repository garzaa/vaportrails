using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HydratedGlyph : MonoBehaviour {
	Sprite originalSprite;
	GameObject textCanvas;
	const int padding = 10;

	static ButtonGlyphMappings mappings;
	Image image;

	void Awake() {
		image = GetComponent<Image>();
		originalSprite = image.sprite;
		if (!mappings) mappings = GameObject.FindObjectOfType<ButtonGlyphMappings>();
		image.enabled = false;
	}

	void Start() {
		CheckGlyph();
	}

	public void CheckGlyph() {
		if (PlayerInput.usingKeyboard) {
			string keyName = mappings.GetKeyName(originalSprite);
			Sprite spriteOverride = mappings.GetKeySprite(keyName);
			if (textCanvas == null) {
				textCanvas = Instantiate(Resources.Load<GameObject>("Runtime/GlyphTextCanvas"), this.transform);
			}
			textCanvas.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
			textCanvas.gameObject.SetActive(true);
			image.enabled = false;

			Text keyNameText = textCanvas.GetComponentInChildren<Text>(includeInactive: true);
			// the first one will be the override 
			Image spriteOverrideImage = textCanvas.GetComponentsInChildren<Image>(includeInactive: true)[1];
			HorizontalLayoutGroup layoutGroup = textCanvas.GetComponent<HorizontalLayoutGroup>();

			// if there's an override sprite, use that instead (needs unpadded layout)
			if (spriteOverride != null) {
				keyNameText.gameObject.SetActive(false);
				spriteOverrideImage.gameObject.SetActive(true);
				spriteOverrideImage.sprite = spriteOverride;
				spriteOverrideImage.SetNativeSize();
				layoutGroup.padding.left = 0;
				layoutGroup.padding.right = 0;
			} else {
				keyNameText.gameObject.SetActive(true);
				keyNameText.text = keyName;
				spriteOverrideImage.gameObject.SetActive(false);
				layoutGroup.padding.left = padding;
				layoutGroup.padding.right = padding;
			}
			Canvas.ForceUpdateCanvases();
		} else {
			if (textCanvas) textCanvas.SetActive(false);
			image.enabled = true;
			Sprite s = mappings.GetGlyph(originalSprite);
			if (s == null) return;
			GetComponent<Image>().sprite = s;
		}
	}
}
