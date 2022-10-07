using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour {
	Animator animator;
	SlowRenderer slowRenderer;

	EntityController currentPlayer;
	
	#pragma warning disable 0649
	[SerializeField] Image speakerPortrait;
	[SerializeField] Text speakerName;
	[SerializeField] GameObject portraitContainer;
	[SerializeField] GameObject speakerNameContainer;
	#pragma warning restore 0649

	Queue<DialogueLine> currentLines = new Queue<DialogueLine>();
	PlayerInput playerInput;

	void Awake() {
		animator = GetComponent<Animator>();
		slowRenderer = GetComponentInChildren<SlowRenderer>();
		// defaults to player 0
		playerInput = gameObject.AddComponent<PlayerInput>();
	}

	void Update() {
		if (playerInput.GenericContinueInput()) {
			if (slowRenderer.rendering) {
				slowRenderer.Complete();
			} else {
				NextLineOrClose();
			}
		}
	}

	public void AddLines(IEnumerable<DialogueLine> lines) {
		foreach (DialogueLine line in lines) {
			currentLines.Enqueue(line);
		}
	}

	void NextLineOrClose() {
		if (currentLines.Count > 0) {
			ShowLine(currentLines.Dequeue());
		} else {
			Close();
		}
	}

	void ShowLine(DialogueLine line) {
		slowRenderer.Render(line.text);
		if (line.portrait) {
			portraitContainer.SetActive(true);
			speakerPortrait.sprite = line.portrait;
		} else {
			portraitContainer.SetActive(false);
		}

		if (!string.IsNullOrEmpty(line.speakerName)) {
			speakerName.text = line.speakerName;
			speakerNameContainer.SetActive(true);
		} else {
			speakerNameContainer.SetActive(false);
		}
	}

	public void Open(EntityController player) {
		if (currentPlayer && currentPlayer!=player) {
			currentPlayer.ExitCutscene(this);
		} 
		player.EnterCutscene(this);
		currentPlayer = player;
		animator.SetBool("Shown", true);
		NextLineOrClose();
	}

	public void Close() {
		if (currentPlayer) currentPlayer.ExitCutscene(this);
		animator.SetBool("Shown", false);
	}
}
