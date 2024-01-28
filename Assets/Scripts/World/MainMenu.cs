using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
	public Beacon newGameBeacon;

	public GameObject continueButton;
	readonly JsonSaver jsonSaver = new();
	SaveManager saveManager;


	public GameCheckpointLoader dummyCheckpoint;

	public void Start() {
		continueButton.SetActive(jsonSaver.HasFile(1));
		saveManager = FindObjectOfType<SaveManager>();
		FindObjectOfType<SpeedrunTimer>().GetComponent<Timer>().Pause();
		FindObjectOfType<SpeedrunTimer>().GetComponent<Timer>().ForceUpdate();
	}

	public void ContinueGame() {
		saveManager.Load();
	}
	
	public void NewGame() {
		saveManager.WipeSave();
		// load a dummy checkpoint to set the flag true and keep any subsequent ones from loading
		dummyCheckpoint.StartUp();
		FindObjectOfType<SpeedrunTimer>().OnNewGame();
		FindObjectOfType<TransitionManager>().BeaconTransition(newGameBeacon);
	}

	public void Quit() {
		Application.Quit();
	}
}
