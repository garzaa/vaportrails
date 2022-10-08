using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VerticalSiblingNavigation : MonoBehaviour {
	void Start() {
		Selectable[] children = GetComponentsInChildren<Selectable>();
		for (int i=0; i<children.Length; i++) {
			Selectable s = children[i];

			Navigation n = new Navigation();
			n.mode = Navigation.Mode.Explicit;
			n.selectOnUp = children[Mathf.Abs((i-1) % children.Length)];
			n.selectOnDown = children[(i+1) % children.Length];

			s.navigation = n;
		}
	}
}
