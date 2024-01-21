using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VerticalNavigation : MonoBehaviour {
	void Start() {
		Selectable[] children = GetComponentsInChildren<Selectable>();
		for (int i=0; i<children.Length; i++) {
			Selectable s = children[i];

			Navigation n = new() {
				mode = Navigation.Mode.Explicit,
				selectOnUp = children[Mathf.Abs((i - 1) % children.Length)],
				selectOnDown = children[(i + 1) % children.Length]
			};

			if (i == 0) {
				n.selectOnUp = children[^1];
			}

			s.navigation = n;
		}
	}
}
