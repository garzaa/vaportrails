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
		target.transform.position = col.bounds.ClosestPoint(player.transform.position);
	}
}
