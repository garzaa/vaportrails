using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class BossFightIntro : MonoBehaviour {
	Animator animator;
	public string bossName;
	public string bossTitle;
	public Sprite bossPortrait;

	public UnityEvent OnContinue;
	
	Text bossNameText;
	Text bossTitleText;
	Image bossPortraitImage;

	EntityController player;
	PlayerInput playerInput;

	GameObject canvas;

	public bool ready = false;
	public bool running = false;
	
	void Start() {
		animator = GetComponent<Animator>();
		bossNameText = transform.Find("Canvas/MinScreenArea/BossName").GetComponent<Text>();
		bossTitleText = transform.Find("Canvas/MinScreenArea/BossTitle").GetComponent<Text>();
		bossPortraitImage = transform.Find("Canvas/BossPortrait").GetComponent<Image>();
		canvas = GetComponentInChildren<Canvas>(includeInactive: true).gameObject;

		bossNameText.text = bossName;
		bossTitleText.text = bossTitle;
		bossPortraitImage.sprite = bossPortrait;

		Run();
	}

	void Update() {
		if (running && ready && playerInput.GenericContinueInput()) {
			Exit();
		}
	}

	void Run() {
		running = true;
		ready = false;
		playerInput = PlayerInput.GetPlayerOneInput();
		player = playerInput.GetComponent<EntityController>();
		player.EnterCutscene(this.gameObject);
	}

	// called from animation
	public void EnableExiting() {
		ready = true;
	}

	public void Exit() {
		animator.SetTrigger("Exit");
	}

	public void FinishExitAnimation() {
		running = false;
		ready = false;
		player.ExitCutscene(this.gameObject);
		OnContinue.Invoke();
	}
}
