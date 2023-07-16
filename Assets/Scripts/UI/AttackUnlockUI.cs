using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ItemPane))]
public class AttackUnlockUI : MonoBehaviour {
	Dictionary<AttackDirection, Sprite> directionSprites;
	Dictionary<int, Sprite> attackButtonSprites;

	ItemPane itemPane;
	Canvas canvas;
	PlayerInput input;

	bool canContinue = false;

	void Start() {
		canvas = GetComponentInChildren<Canvas>();
		itemPane = GetComponent<ItemPane>();
		input = PlayerInput.GetPlayerOneInput();
		canvas.gameObject.SetActive(false);
	}

	void Update() {
		if (canContinue && input.GenericContinueInput()) {
			canvas.gameObject.SetActive(false);
			input.GetComponent<Entity>().ExitCutscene(gameObject);
			canContinue = false;
			CutsceneQueue.OnCutsceneFinish();
		}
	}

	public void Show(Item item) {
		CutsceneQueue.Add(() => this.ShowUI(item));
	}

	public void ShowUI(Item item) {
		StartCoroutine(AllowContinue(item));
	}

	IEnumerator AllowContinue(Item item) {
		input.GetComponent<Entity>().EnterCutscene(gameObject);

		// wait for the little grab animation to finish
		yield return new WaitForSeconds(0.8f);

		canContinue = false;
		canvas.gameObject.SetActive(false);
		itemPane.SetItem(item);
		canvas.gameObject.SetActive(true);
		
		yield return new WaitForSeconds(1);
		
		canContinue = true;
	}
}
