using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventOnPickup : ItemBehaviour {
	public UnityEvent PickupEvent;
	public override void OnPickup(Item parentItem, Inventory inventory, bool silent) {
		PickupEvent.Invoke();
	}
}
