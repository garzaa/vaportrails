using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class ItemGiverEditor : MonoBehaviour {
    public Item item;

    void OnEnable() {
        if (item == null) {
            return;
        }
        GetComponentInChildren<ItemGiver>().item = this.item;
        GetComponentInChildren<SpriteRenderer>().sprite = this.item.icon;
    }

    void OnValidate() {
        OnEnable();
    }
}
#endif
