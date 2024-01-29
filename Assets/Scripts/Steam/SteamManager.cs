using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if (STEAM || UNITY_EDITOR)
using Steamworks;
#endif

public class SteamManager : MonoBehaviour {
	const int demoID = 2809690;
	const int mainID = 2801630;

	static SteamManager instance = null;

	// only compile all this bull crap if it's a steam build!
	// https://forum.unity.com/threads/change-scripting-define-symbols-and-build-the-player-with-a-single-button-press.1364115/
	// but also include a separate PLAYER_STEAM flag? STEAM_BUILD || PLAYER_STEAM
	// https://stackoverflow.com/questions/27519104/several-custom-configuration-in-if-directive

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
	}

	void OnDestroy() {
		SteamClient.Shutdown();
	}
#endif
}
