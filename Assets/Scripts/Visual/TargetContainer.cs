using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TargetContainer : MonoBehaviour, IHitListener {
	SpriteRenderer[] targets;

	public bool hideAllOnStart = false;
	public UnityEvent OnAllBreak;

	void Start() {
		targets = GetComponentsInChildren<SpriteRenderer>();

		if (hideAllOnStart) {
			SetTargets(false);
		}
	}

	public void Reset() {
		foreach (SpriteRenderer target in targets) {
			SetTargets(true);
		}
	}

	public void OnHit(AttackHitbox attack) {
		foreach (SpriteRenderer target in targets) {
			if (target.enabled) {
				return;
			}
		}
		OnAllBreak.Invoke();
	}

	void SetTargets(bool b) {
		foreach (SpriteRenderer target in targets) {
			target.enabled = b;
			// also re-enable their hitboxes
			target.GetComponent<Collider2D>().enabled = b;
			// and then potential streaks back to the main box
			target.GetComponent<LineRenderer>().enabled = b;
		}
	}
}
