using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
	[SerializeField] SaveContainer saveContainer;

	TransitionManager transitionManager;

	JsonSaver jsonSaver = new JsonSaver();
	int slot = 1;

	public Save save {
		get {
			return saveContainer.save;
		}
	}

	void Awake() {
		transitionManager = GameObject.FindObjectOfType<TransitionManager>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			Debug.Log("saving...");
			Save();
		} else if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Debug.Log("loading...");
			Load();
		}
	}

	public void Save() {
		foreach (SavedObject o in GameObject.FindObjectsOfType<SavedObject>()) {
			o.BeforeSave();
		}
		save.version = Application.version;
		jsonSaver.SaveFile(save, slot);
	}

	public void Load() {
		saveContainer.SetSave(jsonSaver.LoadFile(slot));
		foreach (SavedObject o in GameObject.FindObjectsOfType<SavedObject>()) {
			o.AfterDiskLoad();
		}
		transitionManager.LoadLastSavedScene();
	}
}
