using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {
	public GameObject impact;
	public bool randomImpactRotation;
	AttackHitbox attackHitbox;

	Rigidbody2D rb2d;

	void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
		attackHitbox = GetComponent<AttackHitbox>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (impact) {
			if (attackHitbox && !attackHitbox.attacksPlayer) {
				if (other.gameObject.CompareTag(Tags.Player)) {
					return;
				}
				// don't let enemy attacks block it
				if (other.GetComponent<AttackHitbox>()) {
					return;
				}
			}
			GameObject i = Instantiate(impact, transform.position, Quaternion.identity, null);
			if (randomImpactRotation) {
				i.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
			}
			Destroy(gameObject);
		}
	}
}
