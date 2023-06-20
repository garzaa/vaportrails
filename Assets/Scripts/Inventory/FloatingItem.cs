using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerTriggeredObject))]
public class FloatingItem : SavedEnabled, IPlayerEnterListener {
	public Item item;
	public Sprite takenSprite;
	public AudioResource pickupSound;
	public GameObject pickupEffect;
	SpriteRenderer spriteRenderer;

	SavedEnabled s;

	void Awake() {
		s = GetComponent<SavedEnabled>();
		if (takenSprite) {
			spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = takenSprite;
			spriteRenderer.enabled = false;
		}
	}

	void Start() {

	}

	public void OnPlayerEnter(Collider2D player) {
		// spawn the effect, play sound
		pickupSound.PlayFrom(gameObject);
		Instantiate(pickupEffect, transform.position, Quaternion.identity, this.transform);

		// persistent-disable the child item with the hitbox
		s.Disable();

		// if there's a taken sprite then do that
		if (takenSprite) {
			spriteRenderer.enabled = true;
		}

		// add to inventory
		player.GetComponentInChildren<Inventory>().AddItem(item);
	}

}
