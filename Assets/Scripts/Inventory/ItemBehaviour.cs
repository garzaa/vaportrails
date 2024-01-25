using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ItemBehaviour : MonoBehaviour {
	[TextArea] public string extraText;

	public abstract void OnPickup(Item parentItem, Inventory inventory, bool silent);
	
	public virtual void OnRemove(Inventory inventory) {}
	public virtual string GetDescription() {
		return extraText;
	}

	public virtual bool HasCutscene() {
		return false;
	}
}
