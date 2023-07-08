using UnityEngine;
using Rewired;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/ControllerGlyphs")]
public class ControllerGlyphs : ScriptableObject {
	[ListDrawerSettings(ShowIndexLabels = true)]
	public List<ControllerGlyph> glyphMaps;

	public Sprite GetSprite(int elementID, AxisRange axisRange) {
		if (axisRange == AxisRange.Negative) return glyphMaps[elementID].negative;
		else return glyphMaps[elementID].positive;
	}
}

[System.Serializable]
public class ControllerGlyph {
	[PreviewField(50, ObjectFieldAlignment.Right)]
	public Sprite positive;

	[PreviewField(50, ObjectFieldAlignment.Right)]
	public Sprite negative;
}
