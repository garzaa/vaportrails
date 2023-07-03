using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/GameCheckpoint")]
public class GameCheckpoint : ScriptableObject {
	[SerializeField] List<Item> items;
	[SerializeField] List<GameFlag> flags;
	[SerializeField] List<GameCheckpoint> composites;

	public List<Item> GetItems() {
		List<Item> newItems = new List<Item>();
		newItems.AddRange(items);
		foreach (GameCheckpoint checkpoint in composites) {
			newItems.AddRange(checkpoint.GetItems());
		}
		return newItems;
	}

	public List<GameFlag> GetGameFlags() {
		List<GameFlag> newFlags = new List<GameFlag>();
		newFlags.AddRange(flags);
		foreach (GameCheckpoint checkpoint in composites) {
			newFlags.AddRange(checkpoint.GetGameFlags());
		}
		return newFlags;
	}
}
