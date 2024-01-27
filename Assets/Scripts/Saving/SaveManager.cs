using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
	[SerializeField] SaveContainer saveContainer;

	// this one is persisted automatically and used by every save
	// e.g. for things like achievements or other global settings
	Save eternalSave = new();
	const int eternalNum = -1;

	TransitionManager transitionManager;

	readonly JsonSaver jsonSaver = new JsonSaver();
	int slot = 1;

	void Awake() {
		transitionManager = GameObject.FindObjectOfType<TransitionManager>();
		// load a slot zero save if it exists
		if (jsonSaver.HasFile(eternalNum)) {
			eternalSave = jsonSaver.LoadFile(eternalNum);
		}
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.StartUp();
		}
	}

	public Save GetSaveFor(SavedObject o) {
		if (o.useEternalSave) {
			return eternalSave;
		} else {
			return saveContainer.save;
		}
	}

#if UNITY_EDITOR
	void Update() {
		if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			Save();
		} else if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Load();
		}
	}
#endif

	public void Save() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.SyncToRuntime();
		}
		WriteEternalSave();
		FindObjectOfType<MapFog>()?.Save();
		saveContainer.save.version = Application.version;
		jsonSaver.SaveFile(saveContainer.save, slot);
		FindObjectOfType<TimeSinceSave>().OnSave();
	}

	public void Load() {
		WriteEternalSave();
		StartCoroutine(FadeAndLoad());
	}

	IEnumerator FadeAndLoad() {
		transitionManager.FadeToBlack();
		yield return new WaitForSeconds(0.5f);
		saveContainer.SetSave(jsonSaver.LoadFile(slot));
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			// when loading something like playerposition, if it's enabled don't jerk camera around
			o.AfterDiskLoad();
		}
		transitionManager.LoadLastSavedScene();
	}

	public string GetSaveFolderPath() {
        return jsonSaver.GetFolderPath(slot);
    }

	public void WipeSave() {
		saveContainer.save.Wipe();
	}

	public void WriteEternalSave() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			if (o.useEternalSave) o.SyncToRuntime();
		}
		eternalSave.version = Application.version;
		jsonSaver.SaveFile(eternalSave, eternalNum);
	}

	public void OnApplicationQuit() {
		WriteEternalSave();
	}

	public void TransitionPrep() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.SyncToRuntime();
		}
		WriteEternalSave();
		FindObjectOfType<MapFog>()?.Save();
	}
}
