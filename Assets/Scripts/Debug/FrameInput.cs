using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct FrameInput {
	public Dictionary<int, int> actionIDAxes;
	public List<int> actionIDs;

	public FrameInput(Dictionary<int, int> d, List<int> l) {
		actionIDAxes = d;
		actionIDs = l;
	}
}
