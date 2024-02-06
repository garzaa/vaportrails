using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableForSteam : MonoBehaviour {
	public bool setDisabled = false;

	void Start() {
		bool b = false;
#if STEAM
		b = true;
#endif
		if (setDisabled) gameObject.SetActive(!b);
		else gameObject.SetActive(b);
	}
}
