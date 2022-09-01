using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct FrameInput {
	public Dictionary<int, int> axisValues;
	public List<int> buttons;

	public FrameInput(Dictionary<int, int> d, List<int> l) {
		axisValues = d;
		buttons = l;
	}
}
