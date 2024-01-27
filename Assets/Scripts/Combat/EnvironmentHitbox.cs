using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentHitbox : AttackHitbox {
	override protected void Start() {
		base.Start();
		singleHitPerActive = false;
	}

	protected override bool HitsInCutscene() {
		return true;
	}
}
