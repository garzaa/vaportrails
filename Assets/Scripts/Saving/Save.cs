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
		Debug.Log("loading at path "+rawPath);
		// turn a/b/c into [a][b][c]
		List<string> path = new List<string>(rawPath.Split('/'));
		string rootNode = path[0];

		string s = "";
		foreach (var kvp in objects) {
			s += kvp.Key + ", ";
		}
		Debug.Log(s);
		s = "";

		AddDictionary(objects, rootNode);

		// traverse the tree
		Dictionary<string, object> current = objects[rootNode] as Dictionary<string, object>;
		for (int i=1; i<path.Count; i++) {
			foreach (var kvp in current) {
				s += kvp.Key + ", ";
			}
			Debug.Log("looking in "+path[i-1]+" for "+path[i]+", it contains: "+s);
			s = "";
			current = GetSubDict(current, path[i]);
		}
		foreach (var kvp in current) {
			s += kvp.Key + ", ";
		}
		Debug.Log("returning: "+s);
		s = "";
		return current;
	}

	Dictionary<string, object> GetSubDict(Dictionary<string, object> current, string key) {
		AddDictionary(current, key);
		Debug.Log(current[key].GetType());
		if (current[key].GetType().Equals(typeof(JObject))) {
			// if it's loaded from a loosely-typed disk save
			Debug.Log("loading JObject");
			current[key] = (current[key] as JObject).ToObject<Dictionary<string, object>>();
		}

		return current[key] as Dictionary<string, object>;
	}

	void AddDictionary(Dictionary<string, object> d, string keyName) {
		if (!d.ContainsKey(keyName)) {
			Debug.Log("adding dict key "+keyName);
			d[keyName] = new Dictionary<string, object>();
		}
	}
}
