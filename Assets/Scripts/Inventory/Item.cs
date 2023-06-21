using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Data/Item")]
public class Item : ScriptableObject {
	public Sprite icon;
	public Sprite detail;
	public int cost = 0;

	[TextArea]
	public string description;

	public GameObject itemInfo;

	public void OnPickup(GameObject player, bool quiet) {
		if (itemInfo) {
			foreach (IPickup p in itemInfo.GetComponents<IPickup>()) {
				p.OnPickup(player, quiet);
			}
		}
	}
	
}
