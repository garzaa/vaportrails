using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggeredObject : MonoBehaviour {
	List<IPlayerEnterListener> listeners;
	
	void Start() {
		listeners = new List<IPlayerEnterListener>(GetComponentsInParent<IPlayerEnterListener>());
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag(Tags.Player)) {
			OnPlayerEnter(other);
			foreach (IPlayerEnterListener l in listeners) {
				l.OnPlayerEnter(other);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag(Tags.Player)) {
			OnPlayerExit(other);
		}
	}

	protected virtual void OnPlayerEnter(Collider2D player) {}

	protected virtual  void OnPlayerExit(Collider2D player) {}
}
