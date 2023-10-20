using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameFlagChecker : MonoBehaviour {
	public GameFlag flag;

	public UnityEngine.Events.UnityEvent OnPass;
	public UnityEngine.Events.UnityEvent OnFail;

	public void Check() {
		GameFlags flags = FindObjectOfType<GameFlags>();
		bool b = flags.Has(flag);
		if (b) OnPass.Invoke();
		else OnFail.Invoke();
	}
}
