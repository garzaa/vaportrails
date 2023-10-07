using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class PlayerTargetingSystem : MonoBehaviour {
	public GameObject targetReticle;
	public bool targetsPlayer;

	GameObject currentTarget;
	HashSet<GameObject> targetsInRange = new HashSet<GameObject>();

	[Tooltip("Fired when a new object is targeted")]
	public UnityEvent OnTarget;
	[Tooltip("Fired when no objects are targeted")]
	public UnityEvent OnDetarget;


	void OnTriggerEnter2D(Collider2D other) {
		// if it's a player, don't add it
		if (other.CompareTag(Tags.Player) ^ targetsPlayer) return;
		if (!other.GetComponent<Hurtbox>()) return;
		if (other.GetComponent<Hurtbox>().invisibleToTargeters) return;
		targetsInRange.Add(other.gameObject);
	}

	void OnTriggerExit2D(Collider2D other) {
		targetsInRange.Remove(other.gameObject);
	}

	void Update() {
		UpdateCurrentTarget();
		MoveTargetReticle();
	}

	void UpdateCurrentTarget() {
		GameObject closest = GetClosestTarget(transform);
		
		if (currentTarget && !closest) {
			OnDetarget.Invoke();
		} else if (closest != currentTarget) {
			OnTarget.Invoke();
		} else if (!closest && !currentTarget) {
			OnDetarget.Invoke();
		}

		currentTarget = closest;
	}

	void MoveTargetReticle() {
		if (currentTarget == null) {
			ResetReticle();
		}
		else { 
			targetReticle.transform.position = currentTarget.transform.position;
		}
	}

	void ResetReticle() {
		// player looks left by default
		targetReticle.transform.position = transform.position + new Vector3(-transform.lossyScale.x, 0, 0);
	}

	public void OnDisable() {
		targetsInRange.Clear();
		ResetReticle();
		OnDetarget.Invoke();
	}

	GameObject GetClosestTarget(Transform gunPos) {
		float maxDistance = float.PositiveInfinity;
		GameObject nearest = null;
		
		if (targetsInRange.Count == 0) return null;

		// GC
		foreach (GameObject g in targetsInRange) {
			float distance = (g.transform.position - this.transform.position).sqrMagnitude;
			if ((distance < maxDistance) && CanSee(g)) {
				nearest = g;
				maxDistance = distance;
			}
		}

		if (nearest == null) {
			targetsInRange.Remove(nearest);
			return null;
		}

		Hurtbox hurtbox = nearest.GetComponent<Hurtbox>();
		if (hurtbox.useParentTargetingPosition) {
			nearest = hurtbox.GetTargetPosition();
		}

		return nearest;
	}

	bool CanSee(GameObject g) {
		RaycastHit2D groundBlock = Physics2D.Linecast(
			this.transform.position, 
			g.transform.position,
			Layers.GroundMask
		);
		return groundBlock.collider == null;
	}
}
