using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AbilityUnlockUI : MonoBehaviour {
	ItemPane itemPane;
	Canvas canvas;
	PlayerInput input;

	bool canContinue = false;

	void Start() {
		canvas = GetComponentInChildren<Canvas>();
		itemPane = GetComponent<ItemPane>();
		input = PlayerInput.GetPlayerOneInput();
		canvas.gameObject.SetActive(false);
		canvas.GetComponent<DynamicCanvasScaler>().ForceUpdate();
	}

	void Update() {
		if (canContinue && input.GenericContinueInput()) {
			Time.timeScale = 1f;
			canvas.gameObject.SetActive(false);
			input.GetComponent<Entity>().ExitCutscene(gameObject);
			input.GetComponent<Animator>().SetTrigger("ResetToIdle");
			FindObjectOfType<CameraZoom>().ResetZoom();
			canContinue = false;
		}
	}

	public void Show(Item item) {
		StartCoroutine(AllowContinue(item));
	}

	IEnumerator AllowContinue(Item item) {
		input.GetComponent<Entity>().EnterCutscene(gameObject);

		yield return new WaitForSecondsRealtime(10f/12f);
		GameObject.FindObjectOfType<CameraShake>().Shake(Vector2.right * 0.5f);
		GameObject.FindObjectOfType<CameraZoom>().ZoomFor(2, 1f);

		// wait for the animation to finish
		yield return new WaitForSecondsRealtime(0.1f);
		Time.timeScale = 0f;

		canContinue = false;
		canvas.gameObject.SetActive(false);
		itemPane.SetItem(item);
		canvas.gameObject.SetActive(true);
		
		yield return new WaitForSecondsRealtime(1);
		
		canContinue = true;
	}
}
