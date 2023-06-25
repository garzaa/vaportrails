using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/Character")]
public class Character : ScriptableObject {
	public AudioClip voice;
	
	[ShowIf("@voice == null")]
	public AudioResource lineStartSound;
}
