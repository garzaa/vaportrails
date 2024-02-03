using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System;

public class SlowRenderer : MonoBehaviour {

	public enum SlowRenderType {
		LETTER = 0,
		WORD   = 1,
	}

	Text target;
	Coroutine renderRoutine;
	public bool rendering => renderRoutine != null;
	string textToRender;
	string[] words;
	int idx;
	static readonly char[] pauses = {'.', '!', ',', '?', '\n'};

	[TextArea] public string initialText = "";

	Action wordCallback;

	public float letterDelay = 0.01f;
	public UnityEvent RenderEnd;
	
	// TODO: actually implement this
	public SlowRenderType renderType = SlowRenderType.LETTER;

	void Awake() {
		target = GetComponent<Text>();
	}

	void OnEnable() {
		if (!string.IsNullOrEmpty(initialText)) {
			Render(initialText);
		}
	}

	public void Render(string t) {
		if (target == null) target = GetComponent<Text>();
		Render(t, null);
	}

	public void Render(string t, Action wordCallback) {
		target.text = "";
		idx = 0;
		textToRender = t;
		this.wordCallback = wordCallback;
		if (renderType == SlowRenderType.LETTER) renderRoutine = StartCoroutine(SlowRenderLetters());
		else renderRoutine = StartCoroutine(SlowRenderWords());
	}

	public void Complete() {
		StopCoroutine(renderRoutine);
		renderRoutine = null;
		target.text = textToRender;
		wordCallback = null;
		RenderEnd.Invoke();
	}

	IEnumerator SlowRenderLetters() {
		wordCallback?.Invoke();
		while (idx < textToRender.Length) {
			if (wordCallback != null && idx > 0 && textToRender[idx-1] == ' ') {
				wordCallback();
			}
			target.text = textToRender.Substring(0, idx+1) + MakeInvisibleText();
			int scalar = 1;
			if (IsPause(textToRender[idx])) {
				scalar = 7;
			}
			idx++;
			yield return new WaitForSecondsRealtime(letterDelay * scalar);
		}
		renderRoutine = null;
		wordCallback = null;
		RenderEnd.Invoke();
		yield break;
	}

	IEnumerator SlowRenderWords() {
		wordCallback?.Invoke();
		words = textToRender.Split(' ');
		while (idx < words.Length) {
			if (wordCallback != null && idx > 0) {
				wordCallback();
			}
			string newText = string.Join(' ', words[..(idx+1)]);
			// add a space afterwards if it's not the last one
			if (idx + 1 < words.Length) {
				newText += " ";
			}
			target.text = newText + MakeInvisibleText();
			idx++;
			yield return new WaitForSecondsRealtime(letterDelay);
		}
	}

	bool IsPause(char c) {
		return pauses.Contains(c);
	}

	string MakeInvisibleText() {
		string invisText;
		if (renderType == SlowRenderType.LETTER) {
			invisText = textToRender[(idx + 1)..];
		} else {
			invisText = string.Join(' ', words[(idx + 1)..]);
		}
		invisText = "<color=#00000000>" + invisText + "</color>";
		return invisText;
	}
}
