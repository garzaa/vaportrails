using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour {
	PlayerInput input;
	EntityController player;

	GameObject pauseUI;

	void Awake() {
		input = PlayerInput.GetPlayerOneInput();
		player = input.GetComponent<EntityController>();
		pauseUI = transform.GetChild(0).gameObject;
		pauseUI.gameObject.SetActive(false);
	}

	void Update() {
		// player is null if player's deactivated in a cutscene
		// cutscene == no pausing
		if (!player) return;
		if (input.ButtonDown(Buttons.PAUSE)) {
			if (!pauseUI.activeSelf && !player.inCutscene) {
				Open();
			} else if (pauseUI.activeSelf) {
				Close();
			}
		}
	}

	void Open() {
		pauseUI.SetActive(true);
		player.EnterCutsceneNoHalt(this);
		Time.timeScale = 0f;
	}

	public void Close() {
		StartCoroutine(_Close());
	}

	IEnumerator _Close() {
		yield return new WaitForEndOfFrame();
		pauseUI.SetActive(false);
		player.ExitCutscene(this);
		EventSystem.current.SetSelectedGameObject(null);
		Time.timeScale = 1f;
	}

	public void Quit() {
		Application.Quit();
	}
}
