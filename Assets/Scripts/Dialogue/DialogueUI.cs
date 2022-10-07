using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour {
	Animator animator;
	SlowRenderer slowRenderer;
	
	#pragma warning disable 0649
	[SerializeField] Image speakerPortrait;
	[SerializeField] Text speakerName;
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
		Open();
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
		speakerPortrait.sprite = line.portrait;
		speakerName.text = line.speakerName;
	}

	public void Open() {
		animator.SetBool("Shown", true);
	}

	public void Close() {
		animator.SetBool("Shown", false);
	}
}
