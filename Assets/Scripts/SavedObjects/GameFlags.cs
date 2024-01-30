using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameFlags : SavedObject {
	HashSet<string> flags = new HashSet<string>();
	GameFlagChangeListener[] changeListeners = null;

	protected override void LoadFromProperties() {
		flags = GetHashSet<string>(nameof(flags));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties[nameof(flags)] = flags;
	}

	public bool Has(GameFlag f) {
		return flags.Contains(f.name);
	}

	public void Add(GameFlag f) {
		flags.Add(f.name);
		CheckFlagChangeListeners();
	}

	public void Add(IEnumerable<GameFlag> f) {
		foreach (GameFlag flag in f) {
			flags.Add(flag.name);
		}
		CheckFlagChangeListeners();
	}

	void CheckFlagChangeListeners() {
		if (changeListeners == null) changeListeners = FindObjectsOfType<GameFlagChangeListener>(includeInactive: true);
		for (int i=0; i<changeListeners.Length; i++) {
			changeListeners[i].CheckEnabled();
		}
	}
}
