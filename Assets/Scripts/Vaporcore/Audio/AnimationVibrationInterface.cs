using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;

public class AnimationVibrationInterface : MonoBehaviour {
	Player player;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().GetPlayer();
	}
	
	public void Vibrate(VibrationPreset preset) {
		if (!PlayerInput.usingKeyboard && GameOptions.Rumble) {
			player.SetVibration(0, preset.strength, preset.duration);
			player.SetVibration(1, preset.strength, preset.duration);
		}
	}
}
