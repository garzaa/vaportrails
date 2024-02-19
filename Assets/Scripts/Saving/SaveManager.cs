using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SaveManager : MonoBehaviour {
	Save save = new();

	// this one is persisted automatically and used by every save
	// e.g. for things like achievements or other global settings
	Save eternalSave = new();
	const int eternalNum = -1;

	TransitionManager transitionManager;

	static JsonSaver jsonSaver;
	static int slot = 1;

	static SaveManager instance;

	public GameObject saveIndicator;
	MapFog mapFog;
	SavedObject[] savedObjects;
	string appVersion;

	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		instance = this;
		transform.parent = null;
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnLevelLoad;
		mapFog = GameObject.FindObjectOfType<MapFog>();
		jsonSaver = new JsonSaver(Application.persistentDataPath);
		appVersion = Application.version;
		savedObjects = FindObjectsOfType<SavedObject>(includeInactive: true);
	}

	public static int GetSlot() {
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
		savedObjects = FindObjectsOfType<SavedObject>(includeInactive: true);
	}

	public static Save GetSaveFor(SavedObject o) {
		if (o.useEternalSave) {
			return instance.eternalSave;
		} else {
			return instance.save;
		}
	}

// #if UNITY_EDITOR
	void Update() {
		if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			Save();
		} else if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Load();
		}
	}
// #endif

	public static void Save() {
		instance.AsyncSave();
	}

	async void AsyncSave() {
		saveIndicator.SetActive(true);
		instance.save.version = appVersion;
		foreach (SavedObject o in savedObjects) {
			o.SyncToRuntime();
		}
// webgl has problem with the system.threading library
#if !UNITY_WEBGL
		await Task.Run(() => {
#endif
			WriteEternalSave();
			mapFog?.Save();
			jsonSaver.SaveFile(instance.save, slot);
#if !UNITY_WEBGL
		});
		// don't just flash the save icon
		await Task.Delay(1000);
#endif
		FindObjectOfType<TimeSinceSave>()?.OnSave();
		saveIndicator.SetActive(false);
	}

	public static void Load() {
		WriteEternalSave();
		instance.StartCoroutine(FadeAndLoad());
	}

	static IEnumerator FadeAndLoad() {
		instance.transitionManager.FadeToBlack();
		yield return new WaitForSeconds(0.5f);
		instance.save = jsonSaver.LoadFile(slot);
		foreach (SavedObject o in FindObjectsOfType<SavedObject>(includeInactive: true)) {
			// when loading something like playerposition, if it's enabled don't jerk camera around
			o.AfterDiskLoad();
		}
		instance.transitionManager.LoadLastSavedScene();
	}

	public static string GetSaveFolderPath() {
        return jsonSaver.GetFolderPath(slot);
    }

	public static void WipeSave() {
		instance.save.Wipe();
	}

	public static void WriteEternalSave() {
		foreach (SavedObject o in instance.savedObjects) {
			if (o.useEternalSave) o.SyncToRuntime();
		}
		instance.eternalSave.version = instance.appVersion;
		jsonSaver.SaveFile(instance.eternalSave, eternalNum);
	}

	public static void TransitionPrep() {
		foreach (SavedObject o in instance.savedObjects) {
			o.SyncToRuntime();
		}
		WriteEternalSave();
		FindObjectOfType<MapFog>()?.Save();
	}
}
