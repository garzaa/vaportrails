using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnGameFlag : GameFlagChangeListener {
	public GameFlag flag;
	public bool disableOnFlag = false;
	public bool waitUntilNextLoad = false;

	GameFlags flags;

	void Start() {
		_CheckEnabled();
	}

	public override void CheckEnabled() {
		if (waitUntilNextLoad) return;
		_CheckEnabled();
	}

	void _CheckEnabled() {
		if (flags == null) flags = FindObjectOfType<GameFlags>();
		if (disableOnFlag) {
			gameObject.SetActive(!flags.Has(flag));
		} else {
			gameObject.SetActive(flags.Has(flag));	
		}
	}
}
