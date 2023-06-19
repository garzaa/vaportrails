using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class BreakableTile : MonoBehaviour, IHitListener {
	public GameObject breakEffect;
	public TileBase brokenTile;

	public void OnHit(AttackHitbox attack) {
		Tilemap t = GetComponentInParent<Tilemap>();
		// spawn the break effect, rotated to attack direction
		GameObject b = Instantiate(breakEffect);
		b.transform.position = transform.position;
		float x = attack.transform.position.x > transform.position.x ? -1 : 1;
		b.transform.rotation = Quaternion.Euler(0, 0, x*Vector2.SignedAngle(Vector2.up, attack.data.GetKnockback(attack, gameObject)));
		b.transform.parent   = null;

		// swap out the parent tilemap for the broken tile instead
		// that will also delete this gameobject
		t.SetTile(t.WorldToCell(transform.position), brokenTile);

		// which means, if the hurtbox has a hit override sound, play it from the parent tilemap
		GetComponent<Hurtbox>().hitSoundOverride.PlayFrom(transform.parent.gameObject);
	}
}
