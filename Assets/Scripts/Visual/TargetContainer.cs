using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TargetContainer : MonoBehaviour, IHitListener {
	Animator[] targets;

	public bool hideAllOnStart = false;
	public UnityEvent OnAllBreak;
	public UnityEvent OnFirstBreak;
	
	bool firstBreak = true;

	void Start() {
		targets = GetComponentsInChildren<Animator>();

		if (hideAllOnStart) {
			SetTargets(false);
		} else {
			SetTargets(true);
		}
	}

	public void Reset() {
		foreach (Animator target in targets) {
			SetTargets(true);
		}
	}

	public void OnHit(AttackHitbox attack) {
		if (firstBreak) {
			OnFirstBreak.Invoke();
			firstBreak = false;
		}
		StartCoroutine(CheckAll());
	}

	IEnumerator CheckAll() {
		yield return new WaitForEndOfFrame();
		foreach (Animator target in targets) {
			if (target.GetComponent<SpriteRenderer>().enabled) yield break;
		}
		OnAllBreak.Invoke();
	}

	void SetTargets(bool b) {
		foreach (Animator target in targets) {
			target.SetTrigger(b ? "Show" : "Hide");
		}
	}
}
