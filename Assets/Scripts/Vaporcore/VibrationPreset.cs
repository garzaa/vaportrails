using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/VibrationPreset")]
public class VibrationPreset : ScriptableObject {
	public float strength;
	public float duration;
}
