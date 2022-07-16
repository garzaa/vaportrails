using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	public int damage;
	public int IASA;
	public bool jumpCancelable;
	public float stunLength = 0.2f;
	public float hitstop = 0.05f;
	public Vector2 knockback = Vector2.one;
	public GameObject hitmarker;
	public AudioResource hitSound;
	public bool hasSelfKnockback;
	[ShowIf(nameof(hasSelfKnockback))]
	public Vector2 selfKnockback;
}
