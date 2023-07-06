using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BounceBlockHurtbox : Hurtbox {
	public float height;
	public AttackData[] attackFilter;

	HashSet<AttackData> filter;

	void Awake() {
		filter = new HashSet<AttackData>(attackFilter);
	}

	public override Vector2? KnockbackOverride(AttackHitbox attack) {
		if (!filter.Contains(attack.data)) {
			return base.KnockbackOverride(attack);
		}

		Rigidbody2D rb2d = attack.GetComponentInParent<Rigidbody2D>();

		// no matter where the entity is, push it up by the same height
		float targetY = transform.position.y + height + 0.5f;
		float hMax = targetY - rb2d.position.y;

		// v0 = √(2G * hMax)
		float vY = Mathf.Sqrt(Mathf.Abs(2f*Physics2D.gravity.y * hMax));
		return new Vector2(rb2d.velocity.x, vY);
	}
}
