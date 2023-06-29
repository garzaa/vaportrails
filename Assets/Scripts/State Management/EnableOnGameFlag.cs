using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnGameFlag : MonoBehaviour {
	public GameFlag flag;
	public bool disableOnFlag = false;

	GameFlags flags;

	void Start() {
		flags = FindObjectOfType<GameFlags>();
		CheckEnabled();
	}

	public void CheckEnabled() {
		if (disableOnFlag) {
			gameObject.SetActive(!flags.Has(flag));
		} else {
			gameObject.SetActive(flags.Has(flag));	
		}
	}
}
