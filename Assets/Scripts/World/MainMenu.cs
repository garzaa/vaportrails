using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
	public Beacon newGameBeacon;

	public GameObject continueButton;
	readonly JsonSaver jsonSaver = new();
	SaveManager saveManager;

	public void Start() {
		continueButton.SetActive(jsonSaver.HasFile(1));
		saveManager = FindObjectOfType<SaveManager>();
	}

	public void ContinueGame() {
		saveManager.Load();
	}
	
	public void NewGame() {
		saveManager.WipeSave();
		FindObjectOfType<TransitionManager>().BeaconTransition(newGameBeacon);
	}

	public void Quit() {
		Application.Quit();
	}
}
