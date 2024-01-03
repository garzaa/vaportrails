using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactor : MonoBehaviour {
	HashSet<Interactable> interactables = new HashSet<Interactable>();
	Rewired.Player player;
	EntityController entity;
	Collider2D trigger;

	void Start() {
		player = GetComponentInParent<PlayerInput>().GetPlayer();
		entity = GetComponentInParent<EntityController>();
		trigger = GetComponent<Collider2D>();
	}


	void OnTriggerEnter2D(Collider2D other) {
		foreach (Interactable i in other.GetComponents<Interactable>()) {
			i.OnEnter();
			interactables.Add(i);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		foreach (Interactable i in other.GetComponents<Interactable>()) {
			i.OnExit();
			interactables.Remove(i);
		}
	}

	void Update() {
		trigger.enabled = !entity.frozeInputs || (entity.frozeInputs && entity.inAttack);

		if (
			(!entity.frozeInputs || (entity.frozeInputs && entity.inAttack))
			&& player.GetButtonDown(RewiredConsts.Action.Interact)
		) {
			// deal with them leaving the collider on this frame as a result of interaction
			foreach (Interactable i in new List<Interactable>(interactables)) {
				if (i.ShouldFlip()) entity.FlipTo(i.GetFlipPoint());
				i.OnInteract(entity);
			}
		}
	}
}
