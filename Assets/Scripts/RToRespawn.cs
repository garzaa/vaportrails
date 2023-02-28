using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RToRespawn : MonoBehaviour {
	GameObject player;

	void Start() {
		player = PlayerInput.GetPlayerOneInput().gameObject;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.R)) {
			player.transform.position = transform.position;
			player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			if (!player.GetComponent<Entity>().facingRight) {
				player.GetComponent<Entity>().Flip();
			}
		}
	}
}
