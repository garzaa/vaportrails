using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BreakableObject : MonoBehaviour, IHitListener {
	public GameObject breakEffect;
	public Sprite brokenSprite;
	
	public void OnHit(AttackHitbox attack) {
		GameObject b = Instantiate(breakEffect);
		b.transform.position = transform.position;
		float x = attack.transform.position.x > transform.position.x ? -1 : 1;
		b.transform.rotation = Quaternion.Euler(0, 0, x*Vector2.SignedAngle(Vector2.up, attack.data.GetKnockback(attack, gameObject)));
		b.transform.parent   = null;
		GetComponent<SpriteRenderer>().sprite = brokenSprite;
		this.enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}
}
