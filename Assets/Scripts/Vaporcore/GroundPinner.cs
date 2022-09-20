using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundPinner : MonoBehaviour {

	public GameObject target;
	public Vector2 direction = new Vector2(0, -1);
	public float maxDistance = 10;
	public bool disableIfMiss = true;

	void Update() {
		RaycastHit2D hit = Physics2D.Raycast(
			transform.position, 
			(direction * transform.lossyScale).normalized.Rotate(this.transform.eulerAngles.z), 		
			maxDistance,
			1 << LayerMask.NameToLayer(Layers.Ground)
		);
		if (hit.transform != null) {
			if (disableIfMiss) target.SetActive(true);
			target.transform.position = hit.point;
		} else {
			if (disableIfMiss) target.SetActive(false);
			else {
				target.transform.localPosition = (((Vector3) direction.normalized.Rotate(transform.eulerAngles.z)) * maxDistance);
			}
		}
	}
}
