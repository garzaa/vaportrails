using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
	[SerializeField] SaveContainer saveContainer;

	JsonSaver jsonSaver = new JsonSaver();
	int slot = 1;

	public Save save {
		get {
			return saveContainer.save;
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			Save();
		} else if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Load();
		}
	}

	public void Save() {
		// serialize current save to json
		// then save to disk
		save.version = Application.version;
	}

	public void Load() {

	}
}
