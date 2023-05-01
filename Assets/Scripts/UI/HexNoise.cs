using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class HexNoise : MonoBehaviour {
	public int columns = 3;
	public int rows = 9;
	public float updateFreq = 0.5f;

	Text text;
	StringBuilder s = new StringBuilder();

	void Start() {
		text = GetComponent<Text>();
		RenderText();
		StartCoroutine(UpdateText());
	}

	IEnumerator UpdateText() {
		for (;;) {
			yield return new WaitForSecondsRealtime(updateFreq);
			RenderText();
		}
	}

	void RenderText() {
		s.Clear();
		for (int i=0; i<rows; i++) {
			for (int j=0; j<columns; j++) {
				s.Append("0x");
				s.Append(Random.Range(int.MinValue, int.MaxValue).ToString("X8"));
				s.Append(" ");
			}
			s.Append("\n");
		}
		text.text = s.ToString();
	}
}
