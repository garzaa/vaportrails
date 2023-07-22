using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AreaMetadataDisplay : MonoBehaviour {
	public SlowRenderer slowRenderer;
	AudioSource renderNoise;

	public List<Text> mapNameText;

	void Start() {
		slowRenderer = GetComponentInChildren<SlowRenderer>();
		renderNoise = GetComponent<AudioSource>();
		renderNoise.enabled = false;
		slowRenderer.GetComponent<Text>().text = "";
		StartCoroutine(SlowRender());
		foreach (Text t in mapNameText) {
			t.text = SceneManager.GetActiveScene().name;
		}
	}

	IEnumerator SlowRender() {
		yield return new WaitForSeconds(1f);
		renderNoise.enabled = true;
		slowRenderer.Render(SceneManager.GetActiveScene().name);
	}
}
