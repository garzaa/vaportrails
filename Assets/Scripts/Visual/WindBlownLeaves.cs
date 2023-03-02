using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class WindBlownLeaves : MonoBehaviour, IWindReceiver {
	// listen to wind
	// set:
	// leaf spawn rate
	// leaf blowing speed
	// leaf blowing direction

	ParticleSystem leaves;

	void Start() {
		leaves = GetComponent<ParticleSystem>();
	}

	/*
		default values
		start velocity: 0.1 (directly mapped to speed)
		noise strength: 0.3
		noise frequency: 0.28
		leaf spawn rate: 5 -> 20

	*/

	public void Wind(float speed, float size, float strength, float dir) {
		// speed max: 80
		// str max: 2
		// size max: use as multiplier? 1000 base

		// wind speed should change:
		// velocity over lifetime at start
		// noise strength
		var v = leaves.velocityOverLifetime;
		// direction: multiplier for v start
		// speed caps at 80, max here should be 14
		v.x = speed/5.7f * Mathf.Sign(dir);
		var n = leaves.noise;
		n.strength = strength/2f;

		// strength: leaf spawn rate
		var e = leaves.emission;
		e.rateOverTime = (strength * 30);// map from 0-2 to 0->20

		var m = leaves.main;
		m.startLifetime = strength * 5;
	}

	public float Map(float from, float to, float from2, float to2, float value) {
		if(value <= from2){
			return from;
		}else if(value >= to2){
			return to;
		}else{
			return (to - from) * ((value - from2) / (to2 - from2)) + from;
		}
	}

}
