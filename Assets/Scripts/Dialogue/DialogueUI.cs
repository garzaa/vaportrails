using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour {
	Animator animator;
	SlowRenderer slowRenderer;
	AudioResource dialogueRenderSound;
	bool open;

	EntityController currentPlayer;
	
	#pragma warning disable 0649
	[SerializeField] Image speakerPortrait;
	[SerializeField] Text speakerName;
	[SerializeField] GameObject portraitContainer;
	[SerializeField] GameObject speakerNameContainer;
	[SerializeField] GameObject speechBubbleTail;
	#pragma warning restore 0649

	Queue<DialogueLine> currentLines = new Queue<DialogueLine>();
	PlayerInput playerInput;

	void Awake() {
		animator = GetComponent<Animator>();
		slowRenderer = GetComponentInChildren<SlowRenderer>();
		// defaults to player 0
		playerInput = PlayerInput.GetPlayerOneInput();
		dialogueRenderSound = Resources.Load<AudioResource>("Runtime/DialogueRenderSound");
	}

	void Update() {
		if (open && playerInput.GenericContinueInput()) {
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
		dialogueRenderSound.PlayFrom(this.gameObject);
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
			speechBubbleTail.SetActive(true);
			// make the box resize to fit the new name
			LayoutRebuilder.ForceRebuildLayoutImmediate(speakerNameContainer.GetComponent<RectTransform>());
		} else {
			speakerNameContainer.SetActive(false);
			speechBubbleTail.SetActive(false);
		}
	}

	public void Open(EntityController player) {
		open = true;
		if (currentPlayer && currentPlayer!=player) {
			currentPlayer.ExitCutscene(this);
		} 
		player.EnterCutscene(this);
		currentPlayer = player;
		animator.SetBool("Shown", true);
		NextLineOrClose();
	}

	public void Close() {
		open = false;
		if (currentPlayer) currentPlayer.ExitCutscene(this);
		animator.SetBool("Shown", false);
		dialogueRenderSound.PlayFrom(this.gameObject);
	}
}
