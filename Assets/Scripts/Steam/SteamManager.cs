using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SteamManager : MonoBehaviour {
	const int demoID = 2809690;

	// only compile all this bull crap if it's a steam build!
	// https://forum.unity.com/threads/change-scripting-define-symbols-and-build-the-player-with-a-single-button-press.1364115/
	// but also include a separate PLAYER_STEAM flag? STEAM_BUILD || PLAYER_STEAM
	// https://stackoverflow.com/questions/27519104/several-custom-configuration-in-if-directive

	void Start() {
		try {
			Steamworks.SteamClient.Init(demoID, true);
		} catch (System.Exception e) {
			Debug.LogError("Steam client failed to start because: "+e.Message);
		}
	}
}
