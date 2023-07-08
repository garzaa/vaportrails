using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

public class ButtonGlyphMappings : MonoBehaviour {
	public List<NameControllerGlyphMapping> glyphMappings;

	public ControllerGlyphs fallback;

	public List<SpriteActionMapping> actionMappings;

	Dictionary<string, ControllerGlyphs> glyphDict;
	List<HydratedGlyph> hydratedGlyphs;
	Dictionary<Sprite, int> actionMap;

	bool keyboardLastFrame = false;
	string guidLastFrame = "";

	List<ControllerTemplateElementTarget> _templateElementTargets = new List<ControllerTemplateElementTarget>();

	void Awake() {
		hydratedGlyphs = new List<HydratedGlyph>(FindObjectsOfType<HydratedGlyph>(includeInactive: true));
		glyphDict = new Dictionary<string, ControllerGlyphs>();
		foreach (NameControllerGlyphMapping mapping in glyphMappings) {
			foreach (string guid in mapping.GUIDs) {
				glyphDict[guid] = mapping.glyphs;
			}
		}

		actionMap = new Dictionary<Sprite, int>();
		foreach (SpriteActionMapping m in actionMappings) {
			actionMap[m.sprite] = m.rewiredAction;
		}
	}

	void FixedUpdate() {
		// put this in the physics loop because why not, it doesn't need to be instant
		if (!keyboardLastFrame && PlayerInput.usingKeyboard) {
			// then call allll the little hydrated glyphs to update with this
			foreach (HydratedGlyph g in hydratedGlyphs) {
				g.CheckGlyph();
			}
		}
		keyboardLastFrame = PlayerInput.usingKeyboard;
	}

	public Sprite GetGlyph(Sprite actionSprite) {
		// https://guavaman.com/projects/rewired/docs/HowTos.html#display-glyph-for-action-template-specific
		Player player = PlayerInput.GetPlayerOneInput().GetPlayer();
		int actionID = actionMap[actionSprite];
		Controller controller = player.controllers.Joysticks[0];
		ActionElementMap aem = player.controllers.maps.GetFirstElementMapWithAction(controller, actionID, true);
		
		// if nothing is mapped, return null
		if (aem == null) return null;

		// vapor trails does not work with racing wheels or other bull crap
		if (!controller.ImplementsTemplate<GamepadTemplate>()) return null;
		IControllerTemplate template = controller.Templates[0];

		// if no element is targeted??
		if (template.GetElementTargets(aem, _templateElementTargets) == 0) return null;

		ControllerTemplateElementTarget target = _templateElementTargets[0];

		// get the controllerglyphs from the glyphdict
		ControllerGlyphs cg = glyphDict[controller.hardwareTypeGuid.ToString()];
		return cg.GetSprite(target.element.id, target.axisRange);
	}

	public string GetKey(Sprite actionSprite) {

		return null;
	}
}

[System.Serializable]
public class NameControllerGlyphMapping {
	public string[] GUIDs;
	public ControllerGlyphs glyphs;
}

[System.Serializable]
public class SpriteActionMapping {
	public Sprite sprite;
	public int rewiredAction;
}
