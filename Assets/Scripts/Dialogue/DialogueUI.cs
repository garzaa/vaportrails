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

	UnityEvent dialogueLineEndEvent;

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
		dialogueLineEndEvent?.Invoke();
		dialogueLineEndEvent = null;
		if (currentLines.Count > 0) {
			ShowLine(currentLines.Dequeue());
		} else {
			Close();
		}
	}

	void ShowFirstLine(bool wasOpen) {
		if(!wasOpen) ShowLine(currentLines.Dequeue());
	}

	void ShowLine(DialogueLine line) {
		bool hasVoice = line.character?.voice;
		if (!hasVoice) {
			if (line.character?.lineStartSound) {
				line.character.lineStartSound.PlayFrom(this.gameObject);
			}
			else {
				dialogueRenderSound.PlayFrom(this.gameObject);
			}
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

		if (line.character) {
			speakerNameContainer.SetActive(true);
			speakerName.text = line.character.name;
			speechBubbleTail.SetActive(true);
			// make the box resize to fit the new name
			LayoutRebuilder.ForceRebuildLayoutImmediate(speakerNameContainer.GetComponent<RectTransform>());
			speakerNameContainer.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			StartCoroutine(SetName(line.character.name));
		} else {
			speakerNameContainer.SetActive(false);
			speechBubbleTail.SetActive(false);
		}
		if (line.eventOnLineEnd) dialogueLineEndEvent = line.callback;
		else line.callback.Invoke();
	}

	IEnumerator SetName(string n) {
		yield return new WaitForEndOfFrame();
		yield return new WaitForSecondsRealtime(0.2f);
		speakerNameContainer.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;
	}

	public void OpenFrom(GameObject caller) {
		CutsceneQueue.Add(() => this.Open(caller));
	}
 
	void Open(GameObject caller) {
		foreach (EntityController entity in GameObject.FindObjectsOfType<EntityController>()) {
			entity.EnterCutscene(this.gameObject);
		}

		bool wasOpen = open;

		open = true;
		animator.SetBool("Shown", true);
		ShowFirstLine(wasOpen);
		if (dialogueSource) cameraInterface.RemoveFramingTarget(dialogueSource);
		dialogueSource = caller;
		cameraInterface.AddFramingTarget(caller);
	}

	public void Close() {
		open = false;
		foreach (EntityController entity in GameObject.FindObjectsOfType<EntityController>()) {
			entity.ExitCutscene(this.gameObject);
		}
		animator.SetBool("Shown", false);
		dialogueRenderSound.PlayFrom(this.gameObject);
		cameraInterface.RemoveFramingTarget(dialogueSource);
		CutsceneQueue.OnCutsceneFinish();
	}

	public void AddLines(ConversationContainer container) {
		foreach (DialogueLine line in container.GetNextConversation()) {
			currentLines.Enqueue(line);
		}
	}
}
