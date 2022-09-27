using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Ghostfile {

	public float techPercentage;

	public Dictionary<int, List<WeightedFrameInput>> ghost;

	public Ghostfile(Dictionary<int, List<WeightedFrameInput>> inputs) {
		this.ghost = inputs;
	}
}
