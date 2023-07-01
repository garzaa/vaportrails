using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Data/Item")]
public class Item : ScriptableObject {
	public Sprite icon;
	public int cost = 0;

	[TextArea, SerializeField]
	string description;

	public GameObject infoObject;

	public void OnPickup(Inventory inventory, bool quiet) {
		if (infoObject) {
			foreach (ItemBehaviour i in infoObject.GetComponents<ItemBehaviour>()) {
				i.OnPickup(this, inventory, quiet);
			}
		}
	}
	
	public string GetDescription() {
		if (!infoObject) {
			return description;
		} else {
			StringBuilder sb = new StringBuilder();
			sb.Append(description);
			foreach (ItemBehaviour b in infoObject.GetComponents<ItemBehaviour>()) {
				if (!string.IsNullOrEmpty(b.GetDescription())) {
					sb.Append("\n\n");
					sb.Append(b.GetDescription());
				}
			}
			return sb.ToString();
		}
	}
}
