using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableForSteam : MonoBehaviour {
	void Start() {
		bool b = false;
#if STEAM
		b = true;
#endif
		gameObject.SetActive(b);
	}
}
