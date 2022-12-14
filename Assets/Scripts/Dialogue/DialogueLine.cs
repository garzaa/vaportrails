using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine {
	public string speakerName;
	public Sprite portrait;
	[TextArea] public string text;
	public UnityEvent callback;
	public bool eventOnLineEnd;
}
