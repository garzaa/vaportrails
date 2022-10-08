using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

public class AotTypeEnforcer : MonoBehaviour {
	void Awake() {
		AotHelper.EnsureList<System.Object>();
		AotHelper.EnsureType<Ghost>();
		AotHelper.EnsureType<Ghostfile>();
		AotHelper.EnsureType<FrameInput>();
		AotHelper.EnsureList<FrameInput>();
		AotHelper.EnsureType<WeightedFrameInput>();
		AotHelper.EnsureList<WeightedFrameInput>();
		AotHelper.EnsureDictionary<int, List<WeightedFrameInput>>();
	}
}
