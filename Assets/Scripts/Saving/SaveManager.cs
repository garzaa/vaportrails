using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour {
	Save save = new();

	// this one is persisted automatically and used by every save
	// e.g. for things like achievements or other global settings
	Save eternalSave = new();
	const int eternalNum = -1;

	TransitionManager transitionManager;

	readonly JsonSaver jsonSaver = new JsonSaver();
	int slot = 1;

	static SaveManager instance;

	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		instance = this;
		transform.parent = null;
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnLevelLoad;
	}

	public int GetSlot() {
		return slot;
	}

	void OnLevelLoad(Scene scene, LoadSceneMode mode) {
		transitionManager = GameObject.FindObjectOfType<TransitionManager>();
		// load a slot zero save if it exists
		if (jsonSaver.HasFile(eternalNum)) {
			eternalSave = jsonSaver.LoadFile(eternalNum);
		}
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.StartUp();
		}
	}

	public static Save GetSaveFor(SavedObject o) {
		if (o.useEternalSave) {
			return instance.eternalSave;
		} else {
			return instance.save;
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

	public static void Save() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.SyncToRuntime();
		}
		WriteEternalSave();
		FindObjectOfType<MapFog>()?.Save();
		instance.save.version = Application.version;
		instance.jsonSaver.SaveFile(instance.save, instance.slot);
		FindObjectOfType<TimeSinceSave>()?.OnSave();
	}

	public static void Load() {
		WriteEternalSave();
		instance.StartCoroutine(FadeAndLoad());
	}

	static IEnumerator FadeAndLoad() {
		instance.transitionManager.FadeToBlack();
		yield return new WaitForSeconds(0.5f);
		instance.save = instance.jsonSaver.LoadFile(instance.slot);
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			// when loading something like playerposition, if it's enabled don't jerk camera around
			o.AfterDiskLoad();
		}
		instance.transitionManager.LoadLastSavedScene();
	}

	public string GetSaveFolderPath() {
        return jsonSaver.GetFolderPath(slot);
    }

	public static void WipeSave() {
		instance.save.Wipe();
	}

	public static void WriteEternalSave() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			if (o.useEternalSave) o.SyncToRuntime();
		}
		instance.eternalSave.version = Application.version;
		instance.jsonSaver.SaveFile(instance.eternalSave, eternalNum);
	}

	public void OnApplicationQuit() {
		WriteEternalSave();
	}

	public static void TransitionPrep() {
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			o.SyncToRuntime();
		}
		WriteEternalSave();
		FindObjectOfType<MapFog>()?.Save();
	}
}
