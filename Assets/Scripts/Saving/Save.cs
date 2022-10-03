using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class Save {
	public string version;
	public Dictionary<string, object> objects = new Dictionary<string, object>();

	public Dictionary<string, object> LoadAtPath(string rawPath) {
		// turn a/b/c into [a][b][c]
		List<string> path = new List<string>(rawPath.Split('/'));
		string rootNode = path[0];
		AddDictionary(objects, rootNode);

		// traverse the tree
		Dictionary<string, object> current = objects[rootNode] as Dictionary<string, object>;
		for (int i=1; i<path.Count; i++) {
			current = GetSubDict(current, path[i]);
		}
		return current;
	}

	public void SaveAtPath(string rawPath, Dictionary<string, object> properties) {
		// it's a/b/c/d:type -> properties:dict
		// so convert to a/b/c and then set c[d] to properties
		List<string> pathArr = new List<string>(rawPath.Split('/'));
		string objectName = pathArr[pathArr.Count-1];
		pathArr.Remove(objectName);
		string parentPath = string.Join('/', pathArr);

		LoadAtPath(parentPath)[objectName] = properties;
	}

	Dictionary<string, object> GetSubDict(Dictionary<string, object> current, string key) {
		AddDictionary(current, key);
		if (current[key].GetType().Equals(typeof(JObject))) {
			// if it's loaded from a loosely-typed disk save
			current[key] = (current[key] as JObject).ToObject<Dictionary<string, object>>();
		}

		return current[key] as Dictionary<string, object>;
	}

	void AddDictionary(Dictionary<string, object> d, string keyName) {
		if (!d.ContainsKey(keyName)) {
			d[keyName] = new Dictionary<string, object>();
		}
	}
}
