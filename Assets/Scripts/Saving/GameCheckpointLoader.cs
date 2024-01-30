#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCheckpointLoader : SavedObject {
	bool loadedBefore = false;
	public GameCheckpoint checkpoint;
	public bool loadInBuild = false;

	protected override void LoadFromProperties() {
		// this needs to force a load because otherwise if there are no properties
		// it won't be called
		// but it needs to know there are no properties in order to load
		// not just initialize if there's nothing
		try {
			loadedBefore = Get<bool>(nameof(loadedBefore));
		} catch (KeyNotFoundException) {
			loadedBefore = false;
		}
		Start();
	}

	void Start() {
		Debug.Log(this.name + " loaded before: " +loadedBefore);
		if (!loadInBuild && !Application.isEditor) return;
		if (!loadedBefore) {
			// then add everything
			Inventory inventory = PlayerInput.GetPlayerOneInput().GetComponentInChildren<Inventory>();
			inventory.AddItemsQuietly(checkpoint.GetItems());

			GameFlags f = FindObjectOfType<GameFlags>();
			f.Add(checkpoint.GetGameFlags());

			loadedBefore = true;
		}
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
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
#endif
