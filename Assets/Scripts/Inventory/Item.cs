using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Item : ScriptableObject {
	public Sprite icon;
	public Sprite worldIcon;
	public int cost = 0;
	public bool stackable = true;

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

#if UNITY_EDITOR
	[MenuItem("GameObject/Vapor Trails/Create Item")]
	public static void CreateItem() {
		Item i = CreateInstance<Item>();
		AssetDatabase.CreateAsset(i, "Assets/Resources/Runtime/Items/NewItem.asset");
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = i;
	}
#endif
}
