using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HydratedGlyph : MonoBehaviour {
	Sprite originalSprite;
	GameObject textCanvas;
	const int padding = 10;

	static ButtonGlyphMappings mappings;
	Image image = null;
	Image borderImage = null;

	public List<Sprite> spritesToIgnore;
	HashSet<Sprite> ignoreSprites = null;

	void Initialize() {
		image = GetComponent<Image>();
		originalSprite = image.sprite;
		if (!mappings) mappings = FindObjectOfType<ButtonGlyphMappings>();
		ignoreSprites = new HashSet<Sprite>(spritesToIgnore);
		if (!ignoreSprites.Contains(originalSprite)) {
			// set it to fully transparent, otherwise there will be layout issues
			Color c = image.color;
			c.a = 0;
			image.color = c;
		}

		FindObjectOfType<ButtonGlyphMappings>()?.Register(this);
	}

	void Start() {
		CheckGlyph();
	}

	void OnDestroy() {
		FindObjectOfType<ButtonGlyphMappings>()?.Deregister(this);
	}

	public void CheckGlyph() {
		if (image == null) Initialize();

		if (ignoreSprites.Contains(originalSprite)) return;

		if (textCanvas == null) {
			textCanvas = Instantiate(Resources.Load<GameObject>("Runtime/GlyphTextCanvas"), this.transform);
			textCanvas.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
			textCanvas.gameObject.SetActive(true);
			borderImage = textCanvas.GetComponent<Image>();
		}
		Sprite spriteOverride = null;
		string keyName = "";

		Text keyNameText = textCanvas.GetComponentInChildren<Text>(includeInactive: true);
		// the first one will be the override 
		Image spriteOverrideImage = textCanvas.GetComponentsInChildren<Image>(includeInactive: true)[1];
		HorizontalLayoutGroup layoutGroup = textCanvas.GetComponent<HorizontalLayoutGroup>();

		if (PlayerInput.usingKeyboard) {
			keyName = mappings.GetKeyName(originalSprite);
			spriteOverride = mappings.GetKeySprite(keyName);
		} else {
			spriteOverride = mappings.GetGlyph(originalSprite);
		}

		// if there's an override sprite, use that instead (needs unpadded layout)
		if (spriteOverride != null) {
			borderImage.enabled = false;
			keyNameText.gameObject.SetActive(false);
			spriteOverrideImage.gameObject.SetActive(true);
			spriteOverrideImage.sprite = spriteOverride;
			spriteOverrideImage.SetNativeSize();
			layoutGroup.padding.left = 0;
			layoutGroup.padding.right = 0;
		} else {
			borderImage.enabled = true;
			keyNameText.gameObject.SetActive(true);
			keyNameText.text = keyName;
			spriteOverrideImage.gameObject.SetActive(false);
			layoutGroup.padding.left = padding;
			layoutGroup.padding.right = padding;
		}

		// then rebuild the layout
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
	}
}
