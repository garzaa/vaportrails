using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class WindController : MonoBehaviour {
	public float windSpeed = 10;
	public float windSize = 1000;
	public float windStrength = 0.1f;
	[Range(-1, 1)] public float direction = -1;

	float pWindSpeed, pWindSize, pWindStrength, pDirection;

	List<IWindReceiver> windReceivers;

	void Start() {
		windReceivers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IWindReceiver>().ToList();
	}

	void Update() {
		if (
			pWindSpeed != windSpeed
			|| pWindSize != windSize
			|| pWindStrength != windStrength
			|| pDirection != direction
		) {
			for (int i=0; i<windReceivers.Count; i++) {
				windReceivers[i].Wind(windSpeed, windSize, windStrength, direction);
			}
		}
		pWindSpeed = windSpeed;
		pWindSize = windSize;
		pWindStrength = windStrength;
		pDirection = direction;
	}
}
