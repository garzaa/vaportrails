using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameOptions : MonoBehaviour {
	// player will already be in cutscene when it's opened
	// so just open and close this
	// but also wait for a generic escape input to close and go back to the pause menu
	GameObject canvas;
	PlayerInput input;
	
	public bool IsOpen => canvas.activeSelf;

	public static bool shortHop { get; private set; }
	public static bool secondWind { get; private set; }

	public UnityEvent OnClose;

	void Start() {
		canvas = GetComponentInChildren<Canvas>().gameObject;
		input = PlayerInput.GetPlayerOneInput();
		Load();
		Close();
	}

	public static void Load() {
		shortHop = LoadBool("Short Hop");
		secondWind = LoadBool("Second Wind");
	}

	static bool LoadBool(string name) {
		return PlayerPrefs.GetInt(name, 0) == 1;
	}

	public void Open() {
		canvas.SetActive(true);
	}

	void Update() {
		if (!canvas.activeSelf) return;
		if (input.GenericEscapeInput()) {
			Close();
		}
	}

	void Close() {
		canvas.SetActive(false);
		OnClose.Invoke();
	}
}
