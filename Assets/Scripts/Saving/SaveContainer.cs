using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/Runtime/SaveContainer")]
public class SaveContainer : ScriptableObject {
	Save _save = new();

	public Save save {
		get {
			return _save;
		}
	}

	public void SetSave(Save save) {
		_save = save;
	}
}
