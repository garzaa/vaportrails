using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class BeaconWrapper : MonoBehaviour {
	[SerializeField] Beacon beacon;
	public Beacon GetBeacon => beacon;

	public bool faceRight;

	public UnityEvent OnLoad;

	public Vector3 GetPosition() {
		Vector3 pos = transform.position;
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1, Layers.GroundMask);
		if (hit.collider == null) return pos;
		
		pos.y = hit.point.y + 0.5f;
		return pos;
	}
}
