using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System;

public class SlowRenderer : MonoBehaviour {
	Text target;
	Coroutine renderRoutine;
	public bool rendering => renderRoutine != null;
	string textToRender;
	int letterIndex;
	static readonly char[] pauses = {'.', '!', ',', '?', '\n'};

	[TextArea] public string initialText = "";

	Action wordCallback;

	public float letterDelay = 0.01f;
	public UnityEvent RenderEnd;

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
		letterIndex = 0;
		textToRender = t;
		// this happens on a dialogue line open...why
		this.wordCallback = wordCallback;
		renderRoutine = StartCoroutine(SlowRender());
	}

	public void Complete() {
		StopCoroutine(renderRoutine);
		renderRoutine = null;
		target.text = textToRender;
		wordCallback = null;
		RenderEnd.Invoke();
	}

	IEnumerator SlowRender() {
		if (wordCallback != null) wordCallback();
		while (letterIndex < textToRender.Length) {
			if (wordCallback != null && letterIndex > 0 && textToRender[letterIndex-1] == ' ') {
				wordCallback();
			}
			target.text = textToRender.Substring(0, letterIndex+1) + MakeInvisibleText();
			int scalar = 1;
			if (IsPause(textToRender[letterIndex])) {
				scalar = 7;
			}
			letterIndex++;
			yield return new WaitForSecondsRealtime(letterDelay * scalar);
		}
		renderRoutine = null;
		wordCallback = null;
		RenderEnd.Invoke();
		yield break;
	}

	bool IsPause(char c) {
		return pauses.Contains(c);
	}

	string MakeInvisibleText() {
		string invisText = textToRender.Substring(letterIndex+1);
		invisText = "<color=#00000000>" + invisText + "</color>";
		return invisText;
	}
}
