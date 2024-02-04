using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HealthUpgradeUI : MonoBehaviour {
	Animator animator;
	public Item healthUpgradeItem;

	PlayerInput input;
	bool canContinue = false;

	HP hp;
	Inventory playerInventory;
	int plasmaCount;

	public Text[] texts;

	void Awake() {
		animator = GetComponent<Animator>();
		input = PlayerInput.GetPlayerOneInput();
	}

	public void PlayUnlock(Inventory inventory) {
		CutsceneQueue.Add(() => this.RunAnimation(inventory));
	}

	public void RunAnimation(Inventory inventory) {
		input.GetComponent<Entity>().EnterCutscene(gameObject);
		playerInventory = inventory;
		hp = input.GetComponent<HP>();
		// switch on differing amounts of items
		plasmaCount = inventory.GetCount(healthUpgradeItem);
		animator.SetInteger("Plasmas", plasmaCount);
		animator.SetTrigger("OnPickup");
		DoHealthUpgrade();
	}

	// called from animator at the end of the variable animation cycle
	public void AllowContinue() {
		canContinue = true;
	}

	void CloseUI() {
		input.GetComponent<Entity>().ExitCutscene(gameObject);
		canContinue = false;
		animator.Play("Idle");
		CutsceneQueue.OnCutsceneFinish();
	}

	void Update() {
		if (canContinue && input.GenericContinueInput()) {
			CloseUI();
		}
	}

	void DoHealthUpgrade() {
		if (plasmaCount == 3) {
			playerInventory.RemoveItem(healthUpgradeItem, 3);
			hp.SetMax(hp.GetMax() + 4);
			hp.FullHeal();
			foreach (Text t in texts) {
				t.text = "MAX HP INCREASED TO " + hp.GetCurrent();
			}
		}
	}
}
