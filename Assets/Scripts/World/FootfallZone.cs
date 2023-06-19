using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FootfallZone : MonoBehaviour {
	public AudioResource footfallSound;

	void Start() {
		GetComponent<Collider2D>().isTrigger = true;
		gameObject.layer = LayerMask.NameToLayer(Layers.FootfallZones);
	}
}
