#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCheckpointLoader : SavedObject {
	bool loadedBefore = false;
	public GameCheckpoint checkpoint;
	public bool loadInBuild = false;

	protected override void LoadFromProperties() {
		loadedBefore = Get<bool>(nameof(loadedBefore));
	}

	void Start() {
		// for things like going to the training gym
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

	// don't want to always set this crap in the editor
	// so set global namespace to true
	public override string GetObjectPath() {
		return $"global/{name}/{GetType().Name}";
	}
}
#endif
