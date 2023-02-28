using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine {
	// add a character with name, portrait selections, font?
	public string speakerName;
	public Sprite portrait;
	[TextArea] public string text;
	public UnityEvent callback;
	public bool eventOnLineEnd;
}
