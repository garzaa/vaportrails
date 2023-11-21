using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOptions : MonoBehaviour {
	// player will already be in cutscene when it's opened
	// so just open and close this
	// but also wait for a generic escape input to close and go back to the pause menu
	GameObject canvas;
	PlayerInput input;
	public Text savePathText;

	static GameObject timer;
	
	public bool IsOpen => canvas.activeSelf;

	public static bool ShortHop { get; private set; }
	public static bool SecondWind { get; private set; }
	public static float Lookahead { get; private set; }

	public UnityEvent OnClose;

	void Start() {
		canvas = GetComponentInChildren<Canvas>().gameObject;
		input = PlayerInput.GetPlayerOneInput();
		Load();
		Close();
		savePathText.text = "Need to mess with your saves? Find them here:\n"+Application.persistentDataPath;
	}

	public static void Load() {
		// get the canvas child of the timer so the time actually updates
		if (timer == null) timer = FindObjectOfType<SpeedrunTimer>().transform.GetChild(0).gameObject;
		ShortHop = LoadBool("Short Hop");
		SecondWind = LoadBool("Second Wind");
		Lookahead = PlayerPrefs.GetInt("Lookahead", 5) / 5f;
		Application.runInBackground = LoadBool("Run in Background");
		FindObjectOfType<FPSCounter>(includeInactive: true).gameObject.SetActive(LoadBool("FPSCounter"));
		timer.SetActive(LoadBool("Speedrun Timer"));
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

	public void Close() {
		canvas.SetActive(false);
		OnClose.Invoke();
	}
}
