using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[System.Serializable]
public class DialogueLine {
	// add a character with name, portrait selections, font?
	public Character character;

	[HideIf("@character != null")]
	public string speakerName;

	public Sprite portrait;
	[TextArea] public string text;
	public UnityEvent callback;
	public bool eventOnLineEnd;

	public string GetSpeakerName() {
		if (string.IsNullOrWhiteSpace(speakerName) && character) {
			return character.name;
		}
		else return "";
	}
}
