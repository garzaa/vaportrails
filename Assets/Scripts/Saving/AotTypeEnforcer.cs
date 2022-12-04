using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

// https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Fix-AOT-using-AotHelper
public class AotTypeEnforcer : MonoBehaviour {
	void Awake() {
        AotHelper.EnsureList<int>();
	}
}
