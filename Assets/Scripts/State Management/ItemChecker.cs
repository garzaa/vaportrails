using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemChecker : MonoBehaviour {
	public Item item;
	public int count = 1;

	public UnityEngine.Events.UnityEvent OnPass;
	public UnityEngine.Events.UnityEvent OnFail;

	public void Check() {
		Inventory inv = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
		bool b = inv.Has(item) && inv.GetCount(item) >= count;
		if (b) OnPass.Invoke();
		else OnFail.Invoke();
	}
}
