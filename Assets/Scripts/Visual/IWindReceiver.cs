using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IWindReceiver {
	void Wind(float speed, float size, float strength, float dir) {}
}
