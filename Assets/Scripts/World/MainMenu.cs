using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
	public SceneReference newGameScene;
	
	public void NewGame() {
		GameObject.FindObjectOfType<TransitionManager>().SceneTransition(newGameScene.ScenePath);
	}

	public void Quit() {
		Application.Quit();
	}
}
