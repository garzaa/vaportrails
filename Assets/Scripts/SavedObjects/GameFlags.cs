using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameFlags : SavedObject {
	HashSet<string> flags = new HashSet<string>();

	protected override void LoadFromProperties(bool startingUp) {
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
	}
}
