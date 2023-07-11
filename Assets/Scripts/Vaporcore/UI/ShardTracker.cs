using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShardTracker : MonoBehaviour {
	public Text trackerText;
	public Item shardsItem;

	List<FloatingItem> shards = null;

	void OnEnable() {
		if (shards == null) {
			shards = GameObject.FindObjectsOfType<FloatingItem>()
				.Where(x => x.item.Equals(shardsItem))
				.ToList();
		}
		trackerText.text = "??? / ???";
		Check();
	}

	void Check() {
		int takenShards = 0;
		foreach (FloatingItem shard in shards) {
			if (!shard.IsEnabled()) takenShards++;
		}

		trackerText.text = $"{takenShards} / {shards.Count}";
	}
}
