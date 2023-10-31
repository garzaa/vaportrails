using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeath : MonoBehaviour {
	public TextAsset deathText;
	public Text deathMessage;
	SlowRenderer slowRenderer;
	Animator playerAnimator;

	void Start() {
		slowRenderer = GetComponentInChildren<SlowRenderer>(includeInactive: true);
		playerAnimator = PlayerInput.GetPlayerOneInput().GetComponent<Animator>();
	}

	public void Run(AttackHitbox causeOfDeath) {
		GetComponentInChildren<Canvas>(includeInactive: true).gameObject.SetActive(true);
		GetComponentInChildren<SpriteRenderer>(includeInactive: true).gameObject.SetActive(true);
		Time.timeScale = 0;
		playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
		if (!GameOptions.SecondWind) playerAnimator.Play("ValDie");
		string attackName = causeOfDeath.name;
		if (causeOfDeath.data) attackName = causeOfDeath.data.name;
		slowRenderer.Render(deathText.text + $"\nCause of death: {attackName.ToUpper()}");
		if (GameOptions.SecondWind) deathMessage.text = "NOT TODAY";
		else deathMessage.text = "#NO_WAVE";
	}

	public void OnDeathAnimationFinish() {
		Time.timeScale = 1;
		if (GameOptions.SecondWind) {
			GetComponentInChildren<Canvas>(includeInactive: true).gameObject.SetActive(false);
			GetComponentInChildren<SpriteRenderer>(includeInactive: true).gameObject.SetActive(false);
			playerAnimator.updateMode = AnimatorUpdateMode.Normal;
			playerAnimator.GetComponent<HP>().FullHeal();
			return;
		}
		FindObjectOfType<SaveManager>().Load();
	}
}
