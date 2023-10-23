using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FalseWall : PlayerTriggeredObject {

	#pragma warning disable 0649
	[SerializeField] float childOpacity = 1f;
	float opacityPrevFrame;
	#pragma warning restore 0649

	public AudioResource sound;

	SpriteRenderer[] children;

	protected override void Initialize() {
		children = GetComponentsInChildren<SpriteRenderer>();
	}

	void Update() {
		if (childOpacity != opacityPrevFrame) {
			Color c;
			for (int i=0; i<children.Length; i++) {
				c = children[i].color;
				c.a = childOpacity;
				children[i].color = c;
			}
		}
		childOpacity = opacityPrevFrame;
	}

	protected override void OnPlayerEnter(Collider2D player) {
		GetComponent<Animator>().SetBool("Hidden", true);
		sound?.PlayFrom(gameObject);
	}

	protected override void OnPlayerExit(Collider2D player) {
		GetComponent<Animator>().SetBool("Hidden", false);
	}
}
