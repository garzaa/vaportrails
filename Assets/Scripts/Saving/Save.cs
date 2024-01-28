using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

[System.Serializable]
public class Save {
	public string version;
	public Dictionary<string, object> objects = new Dictionary<string, object>();

	public Dictionary<string, object> LoadAtPath(string rawPath) {
		List<string> path = new List<string>(rawPath.Split('/'));
		Dictionary<string, object> current = objects;
		for (int i=0; i<path.Count; i++) {
			current = GetSubDict(current, path[i]);
		}
		return current;
	}

	Dictionary<string, object> GetSubDict(Dictionary<string, object> current, string key) {
		if (!current.ContainsKey(key)) {
			current[key] = new Dictionary<string, object>();
		} else if (current[key].GetType().Equals(typeof(JObject))) {
			current[key] = (current[key] as JObject).ToObject<Dictionary<string, object>>();
		}
		return current[key] as Dictionary<string, object>;
	}

	public void Wipe() {
		objects.Clear();
	}
}
