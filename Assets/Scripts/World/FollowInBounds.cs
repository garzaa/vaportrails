using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowInBounds : MonoBehaviour {
	Collider2D col;
	GameObject player;
	public GameObject target;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().gameObject;
		col = GetComponent<Collider2D>();
	}

	void Update() {
		if (col.bounds.Contains(player.transform.position)) {
			target.transform.position = player.transform.position;
		} else {
			target.transform.position = col.bounds.ClosestPoint(player.transform.position);
		}
	}
}
