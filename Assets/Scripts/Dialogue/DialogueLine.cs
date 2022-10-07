using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine {
	public string speakerName;
	public Sprite portrait;
	[TextArea] public string text;
}
