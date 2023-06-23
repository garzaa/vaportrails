using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// for tiles with gameobjects that we only want on ground, player-immediate layers
public class NoSpawnWithParallax : MonoBehaviour {
	void Start() {
		if (GetComponentInParent<ParallaxLayer>() != null) {
			Destroy(this.gameObject);
		}
	}	
}
