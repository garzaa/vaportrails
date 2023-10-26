using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeath : MonoBehaviour {
	public TextAsset deathText;
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
		playerAnimator.Play("ValDie");
		string attackName = causeOfDeath.name;
		if (causeOfDeath.data) attackName = causeOfDeath.data.name;
		slowRenderer.Render(deathText.text + $"\nCause of death: {attackName.ToUpper()}");
	}

	public void OnDeathAnimationFinish() {
		Time.timeScale = 1;
		FindObjectOfType<SaveManager>().Load();
	}
}
