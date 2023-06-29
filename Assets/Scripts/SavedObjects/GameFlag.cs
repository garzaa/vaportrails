using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/GameFlag")]
public class GameFlag : ScriptableObject {
	[TextArea]
	[SerializeField]
	string editorDescription;
}
