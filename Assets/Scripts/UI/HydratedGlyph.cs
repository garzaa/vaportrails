using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HydratedGlyph : MonoBehaviour {
	Sprite originalSprite;
	bool onCanvas;
	GameObject textCanvas;

	static ButtonGlyphMappings mappings;


	void Awake() {
		onCanvas = GetComponent<RectTransform>() != null;
		if (onCanvas) {
			originalSprite = GetComponent<Image>().sprite;
		} else {
			originalSprite = GetComponent<SpriteRenderer>().sprite;
		}
		if (!mappings) mappings = GameObject.FindObjectOfType<ButtonGlyphMappings>();
	}

	void Start() {
		// get a reference to the glyph canvas prefab here? nahh
		CheckGlyph();
	}

	public void CheckGlyph() {
		if (PlayerInput.usingKeyboard && false) {
			string keyName = mappings.GetKey(originalSprite);
			if (textCanvas == null) textCanvas = Instantiate(Resources.Load<GameObject>("Runtime/GlyphTextCanvas"));
			// then instantiate (if necessary) the canvas and do the WORK
			// then set the canvas to the text string key
		} else {
			if (textCanvas) textCanvas.SetActive(false);
			Sprite s = mappings.GetGlyph(originalSprite);
			if (s == null) return;
			Debug.Log(this.name + " got glyph!!");
			if (onCanvas) GetComponent<Image>().sprite = s;
			else GetComponent<SpriteRenderer>().sprite = s;
		}
	}
}
