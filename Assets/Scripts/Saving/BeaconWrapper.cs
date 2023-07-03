using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BeaconWrapper : MonoBehaviour {
	[SerializeField] Beacon beacon;
	public Beacon GetBeacon => beacon;

	public bool faceRight;
}
