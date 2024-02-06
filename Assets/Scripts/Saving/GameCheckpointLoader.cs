using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCheckpointLoader : SavedObject {
	bool loadedBefore = false;
	public GameCheckpoint checkpoint;
	public bool loadInBuild = false;

	protected override void LoadFromProperties() {
		if (!Application.isEditor) return;
		// this needs to force a load because otherwise if there are no properties
		// it won't be called
		// but it needs to know there are no properties in order to load
		// not just initialize if there's nothing
		try {
			loadedBefore = Get<bool>(nameof(loadedBefore));
		} catch (KeyNotFoundException) {
			loadedBefore = false;
		}

		if (!loadInBuild && !Application.isEditor) return;
		if (!loadedBefore || loadInBuild) {
			// then add everything
			Inventory inventory = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
			inventory.AddItemsQuietly(checkpoint.GetItems());

			GameFlags f = FindObjectOfType<GameFlags>();
			f.Add(checkpoint.GetGameFlags());

			loadedBefore = true;
		}
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		if (!Application.isEditor) return;
		properties[nameof(loadedBefore)] = loadedBefore;
	}

	// the same game checkpoint loaders can have different object names
	// which is why we override it here
	public override string GetObjectPath() {
		return $"global/{GetType().Name}";
	}

	protected override bool ForceLoadIfNoProperties() {
		return true;
	}
}
