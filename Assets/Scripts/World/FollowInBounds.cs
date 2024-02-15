using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowInBounds : MonoBehaviour {
	Collider2D col;
	GameObject player;
	public GameObject target;
	public float tolerance = 0;
	public bool onlyX = false;

	Vector3 pos;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().gameObject;
		col = GetComponent<Collider2D>();
	}

	void Update() {
		pos = col.bounds.ClosestPoint(player.transform.position);
		if (onlyX) pos.x = target.transform.position.x;
		if (Vector2.Distance(pos, target.transform.position) < tolerance) return;
		target.transform.position = pos;
	}
}
