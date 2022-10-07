using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlowRenderer : MonoBehaviour {

	Text target;
	Coroutine renderRoutine;
	public bool rendering => renderRoutine != null;
	string textToRender;
	int letterIndex;
	const float letterDelay = 0.01f;
	static readonly char[] pauses = {'.', '!', ',', '?', '\n'};

	void Awake() {
		target = GetComponent<Text>();
	}

	public void Render(string t) {
		target.text = "";
		letterIndex = 0;
		textToRender = t;
		renderRoutine = StartCoroutine(SlowRender());
	}

	public void Complete() {
		StopCoroutine(renderRoutine);
		renderRoutine = null;
		target.text = textToRender;
	}

	IEnumerator SlowRender() {
		while (letterIndex < textToRender.Length) {
			target.text = textToRender.Substring(0, letterIndex+1) + MakeInvisibleText();
			int scalar = 1;
			if (IsPause(textToRender[letterIndex])) {
				scalar = 7;
			}
			letterIndex++;
			yield return new WaitForSecondsRealtime(letterDelay * scalar);
		}
		renderRoutine = null;
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
