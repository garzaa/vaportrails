using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	[SerializeField] int damage;

	public bool autolink = false;
	[HideIf(nameof(autolink))]
	[SerializeField] Vector2 knockback = Vector2.one;
	[HideIf(nameof(autolink))]
	public bool lateHit = false;

	public int IASA;
	public bool jumpCancelable;
	public float stunLength = 0.5f;
	public float hitstop = 0.05f;
	public GameObject hitmarker;
	public AudioResource hitSound;


	public bool hasSelfKnockback;
	[ShowIf(nameof(hasSelfKnockback))]
	public Vector2 selfKnockback;

	public bool zoomIn = false;

	float enableTime;

	// halve in damage/knockback every 1 frame 
	public int GetDamage() {
		if (!lateHit) return damage;
		return Mathf.Max(damage * 1/FramesOut(), 1);
	}

	public Vector2 GetKnockback(AttackHitbox self, GameObject target) {
		Vector2 v = knockback;

		if (autolink) {
			v = (self.GetComponentInParent<Rigidbody2D>()?.velocity).GetValueOrDefault(Vector2.zero);
			// and then 20% of the difference between their coordinates
			// use coordinates of the box collider center
			v += (Vector2) (target.transform.position - self.transform.position) * 0.2f;
		}

		if (lateHit) return v * (1/(float) FramesOut());
		return v;
	}

	int FramesOut() {
		return Mathf.Max(1, Mathf.FloorToInt((Time.time - enableTime) * 12f));
	}

	public void OnHitboxOut() {
		enableTime = Time.time;
	}
}
