using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

public class PlayerLookAt : MonoBehaviour {
	
	LookAtConstraint looker;
	ConstraintSource source = new ConstraintSource();

	void Start() {
		looker = GetComponent<LookAtConstraint>();
		looker.enabled = false;
		source.weight = 1;
		looker.SetSource(0, source);
	}

	public void LookAt(GameObject target) {
		source.sourceTransform = target.transform;
		looker.enabled = true;
		looker.SetSource(0, source);
	}

	public void StopLookingAt() {
		looker.enabled = false;
	}
}
