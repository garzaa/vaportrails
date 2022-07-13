using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject {
	public int damage;
	public int IASA;
	public bool jumpCancelable;
	public GameObject hitmarker;
	public AudioResource hitSound;

	public List<TimedImpulse> impulses;

	[System.Serializable]
	public class TimedImpulse {
		public int frame;
		public Vector2 impulse;
	}

}
