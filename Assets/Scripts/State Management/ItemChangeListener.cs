using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ItemChangeListener : MonoBehaviour {
	public abstract void OnItemAdd();
	public abstract void OnItemRemove();
}
