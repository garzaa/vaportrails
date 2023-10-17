using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

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
	List<EntityController> players;
	
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
		players = FindObjectsOfType<EntityController>().ToList<EntityController>();
		foreach (EntityController p in players) {
			p.EnterCutscene(gameObject);
		}

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
		foreach (EntityController p in players) {
			p.ExitCutscene(gameObject);
		}
		OnContinue.Invoke();
	}
}
