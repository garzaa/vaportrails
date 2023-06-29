using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableOnGameFlag : MonoBehaviour {
	public GameFlag flag;
	public bool disableOnFlag = false;

	void Start() {
		CheckEnabled();
	}

	public void CheckEnabled() {
		if (disableOnFlag) {
			gameObject.SetActive(!FindObjectOfType<GameFlags>().Has(flag));
		} else {
			gameObject.SetActive(FindObjectOfType<GameFlags>().Has(flag));	
		}
	}
}
