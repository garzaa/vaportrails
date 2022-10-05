using UnityEngine;
using System.Collections.Generic;

public class ItemDB {
    const string itemPath = "Runtime/Items/";

    public static Item GetItem(string itemName) {
        return Resources.Load(itemPath+itemName) as Item;
    }
}
