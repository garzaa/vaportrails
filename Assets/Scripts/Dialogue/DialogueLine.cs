using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[System.Serializable]
public class DialogueLine {
	public Character character;

	public Sprite portrait;
	[TextArea] public string text;
	public UnityEvent callback;
	public bool eventOnLineEnd;
}
