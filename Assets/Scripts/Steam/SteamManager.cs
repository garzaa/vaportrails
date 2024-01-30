using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if (STEAM || UNITY_EDITOR)
using Steamworks;
#endif

public class SteamManager : MonoBehaviour {
	const int demoID = 2809690;
	const int mainID = 2801630;

	static SteamManager instance = null;

	// only compile all this bull crap if it's a steam build!
	// https://forum.unity.com/threads/change-scripting-define-symbols-and-build-the-player-with-a-single-button-press.1364115/

#if (STEAM || UNITY_EDITOR)
	void Awake() {
		if (instance != null) {
			Destroy(this.gameObject);
			return;
		}

		try {
			Steamworks.SteamClient.Init(mainID, true);
		} catch (System.Exception e) {
			Debug.LogError("Steam client failed to start because: "+e.Message);
		}

		// dontdestroyonload only works with root gameobects
		transform.parent = null;
		DontDestroyOnLoad(this.gameObject);
		instance = this;

		#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += TerminateClient;
		#endif
	}

	// do this on application quit, ondestroy shuts it down between scenes and it's not restarted
	#if UNITY_EDITOR
	void TerminateClient(PlayModeStateChange stateChange) {
		if (stateChange == PlayModeStateChange.ExitingPlayMode) {
			SteamClient.Shutdown();
		}
	}
	#endif

	[ContextMenu("Reset Achievements")]
	public void ResetAchievements() {
		Achievement[] achievements = Resources.LoadAll<Achievement>("Runtime/Achievements");
		foreach (Achievement a in achievements) {
			var ach = new Steamworks.Data.Achievement(a.name);
			ach.Clear();
		}
	}
#endif
}
