using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if (STEAM || EDITOR_STEAM)
using Steamworks;
#endif

public class SteamManager : MonoBehaviour {
	#if STEAM_DEMO
		const int appID = 2809690;
	#elif (STEAM || EDITOR_STEAM)
		const int appID = 2801630;
	#endif

	#pragma warning disable 0414
	static SteamManager instance = null;
	#pragma warning restore 0414

	// only compile all this bull crap if it's a steam build!
	// https://forum.unity.com/threads/change-scripting-define-symbols-and-build-the-player-with-a-single-button-press.1364115/

#if (STEAM || EDITOR_STEAM)
	void Awake() {
		if (instance != null) {
			Destroy(this.gameObject);
			return;
		}

		try {
			Steamworks.SteamClient.Init(appID, true);
		} catch (System.Exception e) {
			Debug.LogError("Steam client failed to start because: "+e.Message);
		}

		// dontdestroyonload only works with root gameobects
		transform.parent = null;
		DontDestroyOnLoad(this.gameObject);
		instance = this;
	}

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
