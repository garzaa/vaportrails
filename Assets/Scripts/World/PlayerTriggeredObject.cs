using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggeredObject : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag(Tags.Player)) {
			OnPlayerEnter(other);
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
