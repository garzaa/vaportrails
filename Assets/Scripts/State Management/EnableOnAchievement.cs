using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnAchievement : MonoBehaviour {
	public Achievement cheevo;
	public bool disableInstead = false;

	Achievements achievements;

	void Start() {
		StartCoroutine(CheckEnabled());
	}

	IEnumerator CheckEnabled() {
		yield return new WaitForEndOfFrame();
		if (achievements == null) achievements = FindObjectOfType<Achievements>(includeInactive: true);
		if (disableInstead) {
			gameObject.SetActive(!achievements.Has(cheevo));
		} else {
			gameObject.SetActive(achievements.Has(cheevo));	
		}
	}
}
