using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public struct FrameInput {
	public Dictionary<int, int> actionIDAxes;
	public HashSet<int> actionIDs;

	public FrameInput(Dictionary<int, int> d, HashSet<int> l) {
		actionIDAxes = d;
		actionIDs = l;
	}

	public override bool Equals(object obj) {
		if (obj == null) return false;
		if (Object.ReferenceEquals(obj, this)) return true;
		if (GetType() != obj.GetType()) return false;

		FrameInput other = (FrameInput) obj;

		if (!actionIDs.SetEquals(other.actionIDs)) return false;

		if (actionIDAxes.Count != other.actionIDAxes.Count) return false;
		// this does a set comparison on key-value pairs
		if (actionIDAxes.Except(other.actionIDAxes).Any()) return false;

		return true;
	}

	public override int GetHashCode() {
		int hash = 13;
		hash = (hash * 7) + actionIDAxes.GetHashCode();
		hash = (hash * 13) + actionIDs.GetHashCode();
		return hash;
	}
}
