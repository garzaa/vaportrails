using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	[SerializeField] int damage;

	public bool autolink = false;
	[HideIf(nameof(autolink))]
	[SerializeField] Vector2 knockback = Vector2.one;

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

	[Tooltip("For sweet/sourspots")]
	public AttackData altHitbox;

	[Range(0, 1)]
	public float diRatio = 1f;

	// halve in damage/knockback every 1 frame 
	public int GetDamage() {
		return damage;
	}

	public Vector2 GetKnockback(AttackHitbox self, GameObject target) {
		Vector2 v = knockback;

		if (autolink) {
			v = (self.GetComponentInParent<Rigidbody2D>()?.velocity).GetValueOrDefault(Vector2.zero);
			// and then 20% of the difference between their coordinates
			// use coordinates of the box collider center
			v += (Vector2) (target.transform.position - self.transform.position) * 0.2f;
		}

		return v;
	}

}
