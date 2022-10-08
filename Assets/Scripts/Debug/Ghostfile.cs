using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Ghostfile {
	public Dictionary<int, List<WeightedFrameInput>> ghost;

	// for AOT loading
	public Ghostfile() {}

	public Ghostfile(Dictionary<int, List<WeightedFrameInput>> inputs) {
		this.ghost = inputs;
	}
}
