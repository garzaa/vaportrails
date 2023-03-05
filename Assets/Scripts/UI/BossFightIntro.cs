using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossFightIntro : MonoBehaviour {
	Animator animator;
	public string title;
	public string subtitle;
	public Sprite bossPortrait;
	
	Text titleText;
	Text subtitleText;
	Image bossPortraitImage;

	EntityController player;
	PlayerInput playerInput;

	GameObject canvas;

	bool ready = false;
	bool running = false;
	
	void Start() {
		animator = GetComponent<Animator>();
		titleText = transform.Find("Title").GetComponent<Text>();
		subtitleText = transform.Find("Subtitle").GetComponent<Text>();
		bossPortraitImage = transform.Find("BossPortrait").GetComponent<Image>();
		canvas = GetComponentInChildren<Canvas>(includeInactive: true).gameObject;
	}

	void Update() {
		if (running && ready && playerInput.GenericContinueInput()) {
			Exit();
		}
	}

	void Run() {
		running = true;
		canvas.gameObject.SetActive(true);
		ready = false;
		playerInput = PlayerInput.GetPlayerOneInput();
		player = playerInput.GetComponent<EntityController>();
		player.EnterCutscene(this);
	}

	// called from animation
	public void EnableExiting() {
		ready = true;
	}

	public void Exit() {
		running = false;
		ready = false;
		player.ExitCutscene(this);
		canvas.SetActive(false);
	}
}
