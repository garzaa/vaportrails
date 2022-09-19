using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	[SerializeField] int damage;
	[SerializeField] Vector2 knockback = Vector2.one;
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

	public Vector2 GetKnockback() {
		if (!lateHit) return knockback;
		return knockback * (1/(float) FramesOut());
	}

	int FramesOut() {
		return Mathf.Max(1, Mathf.FloorToInt((Time.time - enableTime) * 12f));
	}

	public void OnHitboxOut() {
		enableTime = Time.time;
	}
}
