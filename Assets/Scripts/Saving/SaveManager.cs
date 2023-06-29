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
			Save();
		} else if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Load();
		}
	}

	public void Save() {
		foreach (SavedObject o in GameObject.FindObjectsOfType<SavedObject>()) {
			o.BeforeSave();
		}
		FindObjectOfType<MapFog>()?.Save();
		save.version = Application.version;
		jsonSaver.SaveFile(save, slot);
	}

	public void Load() {
		StartCoroutine(FadeAndLoad());
	}

	IEnumerator FadeAndLoad() {
		transitionManager.FadeToBlack();
		yield return new WaitForSeconds(0.5f);
		saveContainer.SetSave(jsonSaver.LoadFile(slot));
		foreach (SavedObject o in GameObject.FindObjectsOfType<SavedObject>()) {
			// when loading something like playerposition, if it's enabled don't jerk camera around
			o.AfterDiskLoad();
		}
		transitionManager.LoadLastSavedScene();
	}

	public string GetSaveFolderPath() {
        return jsonSaver.GetFolderPath(slot);
    }
}
