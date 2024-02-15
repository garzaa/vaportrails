using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class PositionMover : MonoBehaviour {
	GameObject currentTarget;
	Animator animator;
	Vector2 targetPos;
	Vector3 s;

	public Vector2 offset = Vector2.zero;
	public float speed = 1;
	public bool sendSpeedToAnimator;
	public bool stopTrackingOnFinish = true;
	public bool flipToMovement;
	public GameObject initialTarget;

	void Start() {
		if (sendSpeedToAnimator) animator = GetComponent<Animator>();
		if (initialTarget) {
			currentTarget = initialTarget;
		}
	}

	void Update() {
		if (currentTarget) {
			animator?.SetFloat("Speed", speed);
			targetPos = (Vector2) currentTarget.transform.position + offset;
			transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
			if (Vector2.Distance(transform.position, targetPos) < 0.1f) {
				if (stopTrackingOnFinish) currentTarget = null;
				animator?.SetFloat("Speed", 0);
			} else {

				// don't reset scale if standing still
				if (flipToMovement) {
					s = transform.localScale;
					s.x = targetPos.x >= transform.position.x ? 1 : -1;
					transform.localScale = s;
				}
			}

		} else {
			animator?.SetFloat("Speed", 0);
		}
	}

	public void MoveTo(GameObject target) {
		currentTarget = target;
	}
}
