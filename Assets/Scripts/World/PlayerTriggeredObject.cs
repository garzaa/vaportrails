using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggeredObject : MonoBehaviour {
	List<IPlayerEnterListener> listeners;
	
	void Start() {
		listeners = new List<IPlayerEnterListener>(GetComponentsInParent<IPlayerEnterListener>());
	}

	bool IsPlayer(Collider2D other) {
		return other.CompareTag(Tags.Player) && other.GetComponent<EntityController>() != null;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (IsPlayer(other)) {
			OnPlayerEnter(other);
			foreach (IPlayerEnterListener l in listeners) {
				l.OnPlayerEnter(other);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (IsPlayer(other)) {
			OnPlayerExit(other);
		}
	}

	protected virtual void OnPlayerEnter(Collider2D player) {}

	protected virtual  void OnPlayerExit(Collider2D player) {}
}
