using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	public int damage;
	public int IASA;
	public bool jumpCancelable;
	public float stunLength = 0.2f;
	public Vector2 knockback = Vector2.one;
	public GameObject hitmarker;
	public AudioResource hitSound;
}
