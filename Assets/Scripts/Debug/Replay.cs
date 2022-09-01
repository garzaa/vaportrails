using System;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Replay {
	public List<FrameInput> frameInputs;

	public Replay(List<FrameInput> inputs) {
		frameInputs = inputs;
	}

	public int length {
		get { return frameInputs.Count; }
	}
}
