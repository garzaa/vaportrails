using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloatingItem : SavedEnabled, IPlayerEnterListener {
	public Item item;
	public Sprite takenSprite;
	public AudioResource pickupSound;
	public GameObject pickupEffect;
	SpriteRenderer spriteRenderer;

	void Awake() {
		if (takenSprite) {
			spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = takenSprite;
			spriteRenderer.enabled = false;
		}
	}

	protected override void LoadFromProperties(bool startingUp) {
		base.LoadFromProperties(startingUp);
		if (!Get<bool>("enabled")) {
			if (takenSprite) {
				spriteRenderer.enabled = true;
			}
		}
	}

	public void OnPlayerEnter(Collider2D player) {
		// spawn the effect, play sound
		pickupSound?.PlayFrom(gameObject);
		if (pickupEffect) {
			Instantiate(pickupEffect, transform.position, Quaternion.identity);
		}

		// persistent-disable the child item with the hitbox
		Disable();

		// if there's a taken sprite then do that
		if (takenSprite) {
			spriteRenderer.enabled = true;
		}

		// add to inventory
		player.GetComponentInChildren<Inventory>().AddItem(item);
	}

}
