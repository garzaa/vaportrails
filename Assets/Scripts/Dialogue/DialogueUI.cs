using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

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
	GameObject dialogueSource;
	CameraInterface cameraInterface;

	UnityEvent queuedEvent;

	void Awake() {
		animator = GetComponent<Animator>();
		slowRenderer = GetComponentInChildren<SlowRenderer>();
		// defaults to player 0
		playerInput = PlayerInput.GetPlayerOneInput();
		dialogueRenderSound = Resources.Load<AudioResource>("Runtime/DialogueRenderSound");
		cameraInterface = GameObject.FindObjectOfType<CameraInterface>();
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
		queuedEvent?.Invoke();
		queuedEvent = null;
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
		if (line.eventOnLineEnd) queuedEvent = line.callback;
		else line.callback.Invoke();
	}

	public void Open(EntityController player, GameObject caller) {
		// TODO: tear out all this multiplayer bullshit and do it for all players (or all human players or something)
		open = true;
		if (currentPlayer && currentPlayer!=player) {
			currentPlayer.ExitCutscene(this);
		} 
		player.EnterCutscene(this);
		currentPlayer = player;
		animator.SetBool("Shown", true);
		NextLineOrClose();
		if (dialogueSource) cameraInterface.RemoveFramingTarget(dialogueSource);
		dialogueSource = caller;
		cameraInterface.AddFramingTarget(caller);
	}

	public void Close() {
		open = false;
		if (currentPlayer) {
			currentPlayer.ExitCutscene(this);
		}
		animator.SetBool("Shown", false);
		dialogueRenderSound.PlayFrom(this.gameObject);
		cameraInterface.RemoveFramingTarget(dialogueSource);
	}
}
