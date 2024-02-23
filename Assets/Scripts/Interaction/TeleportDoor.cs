using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportDoor : Interactable {
	public Sprite icon;
	public AudioResource doorSound;
	public Transform target;

	CameraInterface cameraInterface;

	protected override void Start() {
		base.Start();
		// load the mapIcon prefab base, then set it as the sprite
		if (icon != null) {
			GameObject g = Instantiate(Resources.Load<GameObject>("Runtime/MapIconBase"), transform);
			g.transform.position = transform.position;
			g.GetComponent<SpriteRenderer>().sprite = icon;
		}
	}

	public override void OnInteract(EntityController player) {
		doorSound?.PlayFrom(gameObject);
		// avoid camera moving around too quickly
		FindObjectOfType<CameraInterface>().HardLockFor(2);
		player.transform.position = target.transform.position;
	}

	public override bool ShouldFlip() {
		return false;
	}
}
