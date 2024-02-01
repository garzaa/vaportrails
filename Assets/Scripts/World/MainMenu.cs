using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
	public Beacon newGameBeacon;

	public GameObject continueButton;
	JsonSaver jsonSaver;

#if UNITY_EDITOR
	public GameCheckpointLoader dummyCheckpoint;
#endif

	public void Start() {
		jsonSaver = new JsonSaver(Application.persistentDataPath);
		continueButton.SetActive(jsonSaver.HasFile(1));
		FindObjectOfType<SpeedrunTimer>().GetComponent<Timer>().Pause();
		FindObjectOfType<SpeedrunTimer>().GetComponent<Timer>().ForceUpdate();
	}

	public void ContinueGame() {
		SaveManager.Load();
	}
	
	public void NewGame() {
		SaveManager.WipeSave();
#if UNITY_EDITOR
		// load a dummy checkpoint to set the flag true and keep any subsequent ones from loading
		// need to call Load explictly to refresh the reference to the Save's properties dictionary
		// this is all editor stuff but still
		dummyCheckpoint.Load();
		dummyCheckpoint.StartUp();
#endif
		SaveManager.Save();
		FindObjectOfType<SpeedrunTimer>().OnNewGame();
		FindObjectOfType<TransitionManager>().BeaconTransition(newGameBeacon);
	}

	public void Quit() {
		Application.Quit();
	}
}
