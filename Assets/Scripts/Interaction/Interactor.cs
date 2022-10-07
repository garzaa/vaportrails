using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactor : MonoBehaviour {
	HashSet<Interactable> interactables = new HashSet<Interactable>();
	Rewired.Player player;
	EntityController entity;

	void Start() {
		player = GetComponentInParent<PlayerInput>().GetPlayer();
		entity = GetComponentInParent<EntityController>();
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
		if (
			!entity.frozeInputs
			&& player.GetButtonDown(RewiredConsts.Action.Interact)
		) {
			foreach (Interactable i in interactables) {
				entity.FlipTo(i.gameObject);
				i.OnInteract(entity);
			}
		}
	}
}
