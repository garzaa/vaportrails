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

	AudioSource audioSource;

	void Awake() {
		animator = GetComponent<Animator>();
		slowRenderer = GetComponentInChildren<SlowRenderer>();
		// defaults to player 0
		playerInput = PlayerInput.GetPlayerOneInput();
		dialogueRenderSound = Resources.Load<AudioResource>("Runtime/DialogueRenderSound");
		cameraInterface = GameObject.FindObjectOfType<CameraInterface>();
		audioSource = GetComponent<AudioSource>();
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
		bool hasVoice = line.character?.voice;
		if (!hasVoice) {
			dialogueRenderSound.PlayFrom(this.gameObject);
			slowRenderer.Render(line.text);
		} else {
			audioSource.clip = line.character.voice;
			slowRenderer.Render(line.text, wordCallback: () => audioSource.Play());
		}
		if (line.portrait) {
			portraitContainer.SetActive(true);
			speakerPortrait.sprite = line.portrait;
			speakerPortrait.SetNativeSize();
		} else {
			portraitContainer.SetActive(false);
		}

		if (!string.IsNullOrEmpty(line.speakerName) || line.character) {
			// if speaker name is set, it takes priority - hack for Val to voice the other characters until they get their own voices
			// in the ending cutscene
			if (line.character && string.IsNullOrEmpty(line.speakerName)) {
				speakerName.text = line.character.name;
			} else {
				speakerName.text = line.speakerName;
			}
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

	public void Open(GameObject caller) {
		foreach (EntityController entity in GameObject.FindObjectsOfType<EntityController>()) {
			entity.EnterCutscene(this);
		}
		open = true;
		animator.SetBool("Shown", true);
		NextLineOrClose();
		if (dialogueSource) cameraInterface.RemoveFramingTarget(dialogueSource);
		dialogueSource = caller;
		cameraInterface.AddFramingTarget(caller);
	}

	public void Close() {
		open = false;
		foreach (EntityController entity in GameObject.FindObjectsOfType<EntityController>()) {
			entity.ExitCutscene(this);
		}
		animator.SetBool("Shown", false);
		dialogueRenderSound.PlayFrom(this.gameObject);
		cameraInterface.RemoveFramingTarget(dialogueSource);
	}
}
