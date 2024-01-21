using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour {
	PlayerInput input;
	EntityController player;

	GameObject pauseUI;
	public GameObject quitConfirm;

	public SceneReference mainMenuScene;
	bool blockEscape;

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
		if (input.ButtonDown(Buttons.PAUSE) && !blockEscape) {
			if (!pauseUI.activeSelf && !player.inCutscene) {
				Open();
			} else if (pauseUI.activeSelf) {
				Close();
			}
		}
	}

	void Open() {
		pauseUI.SetActive(true);
		player.EnterCutsceneNoHalt(this.gameObject);
		Time.timeScale = 0f;
	}

	public void Close() {
		StartCoroutine(_Close());
	}

	IEnumerator _Close() {
		yield return new WaitForEndOfFrame();
		pauseUI.SetActive(false);
		player.ExitCutscene(this.gameObject);
		EventSystem.current.SetSelectedGameObject(null);
		Time.timeScale = 1f;
		CancelQuit();
	}

	public void AskForQuitConfirm() {
		quitConfirm.SetActive(true);
	}

	public void CancelQuit() {
		quitConfirm.SetActive(false);
	}

	public void Quit() {
		FindObjectOfType<SaveManager>().WriteEternalSave();
		Application.Quit();
	}

	public void MainMenu() {
		Close();
		FindObjectOfType<TransitionManager>().SceneTransition(mainMenuScene.ScenePath);
	}

	public void BlockEscape() {
		blockEscape = true;
	}

	public void AllowEscape() {
		if (!gameObject.activeSelf) return;
		StartCoroutine(AllowEscNextFrame());
	}
	
	IEnumerator AllowEscNextFrame() {
		yield return new WaitForEndOfFrame();
		blockEscape = false;
		GetComponentInChildren<SelectFirstChild>()?.OnEnable();
	}
}
