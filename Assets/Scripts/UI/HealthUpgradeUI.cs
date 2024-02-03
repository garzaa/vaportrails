using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthUpgradeUI : MonoBehaviour {
	Animator animator;
	public Item healthUpgradeItem;

	PlayerInput input;
	bool canContinue = false;

	void Awake() {
		animator = GetComponent<Animator>();
		input = PlayerInput.GetPlayerOneInput();
	}

	public void PlayUnlock(Inventory inventory) {
		CutsceneQueue.Add(() => this.RunAnimation(inventory));
	}

	public void RunAnimation(Inventory inventory) {
		Time.timeScale = 0;
		// switch on differing amounts of items
		animator.SetInteger("Plasmas", inventory.GetCount(healthUpgradeItem));
		animator.SetTrigger("OnPickup");
	}

	// called from animator at the end of the variable animation cycle
	public void AllowContinue() {
		canContinue = true;
	}

	void Update() {
		if (canContinue && input.GenericContinueInput()) {
			Time.timeScale = 1f;
			input.GetComponent<Entity>().ExitCutscene(gameObject);
			input.GetComponent<Animator>().SetTrigger("ResetToIdle");
			canContinue = false;
			CutsceneQueue.OnCutsceneFinish();
		}
	}
}
