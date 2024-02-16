using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShardTracker : MonoBehaviour {

	[System.Serializable]
	public class TrackedItem {
		public Item item;
		public Text textbox;
	}

	public List<TrackedItem> trackedItems = new();
	Dictionary<TrackedItem, List<FloatingItem>> trackedLists = new();

	bool initialized = false;

	void Initialize() {
		foreach (TrackedItem tracked in trackedItems) {
			trackedLists[tracked] = FindObjectsOfType<FloatingItem>()
				.Where(x => x.item.Equals(tracked.item))
				.ToList();
			
			tracked.textbox.text = "??? / ???";
		}
		initialized = true;
	}

	void OnEnable() {
		if (!initialized) Initialize();
		Check();
	}

	void Check() {
		foreach (TrackedItem tracked in trackedItems) {
			int taken = 0;
			foreach (FloatingItem floatingItem in trackedLists[tracked]) {
				if (!floatingItem.IsEnabled()) taken++;
			}
			tracked.textbox.text = $"{taken} / {trackedLists[tracked].Count}";
		}
	}
}
