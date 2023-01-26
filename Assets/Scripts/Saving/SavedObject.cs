using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System;

public abstract class SavedObject : MonoBehaviour {

	[Tooltip("Use state shared between scenes for objects with this hierarchichal name.")]
	public bool useGlobalNamespace;

	private Dictionary<string, object> properties = new Dictionary<string, object>();

	bool hasSavedData => properties.Count > 0;

	Save save;

	void Start() {
		Load();
		Initialize();
		if (hasSavedData) LoadFromProperties();
	}

	void Load() {
		save = GameObject.FindObjectOfType<SaveManager>().save;
		properties = save.LoadAtPath(GetObjectPath());
	}

	public void BeforeSave() {
		SaveToProperties(ref properties);
	}

	public void AfterDiskLoad() {
		Load();
		if (hasSavedData) LoadFromProperties();
	}

	protected abstract void SaveToProperties(ref Dictionary<string, object> properties);
	protected virtual void Initialize() {}
	protected abstract void LoadFromProperties();

	public string GetObjectPath() {
		if (useGlobalNamespace) return $"global/{name}/{GetType().Name}";
		return $"{SceneManager.GetActiveScene().name}/{gameObject.GetHierarchicalName()}/{GetType().Name}";
	}

	protected T Get<T>(string key) {
		var v = properties[key];
		try {
			return (T) v;
		} catch (InvalidCastException) {
			// simple conversions that are serialized as C# types
			if (typeof(T).Equals(typeof(float))) {
				return (T) Convert.ChangeType(v, typeof(float));
			} else if (typeof(T).Equals(typeof(int))) {
				return (T) Convert.ChangeType(v, typeof(int));
			} else if (properties[key].GetType().Equals(typeof(JObject))) {
				return (properties[key] as JObject).ToObject<T>();
			}
		}
		throw new InvalidCastException("No valid cast for property "+key+"!");
	}

	protected List<T> GetList<T>(string key) {
		var v = properties[key];
		try {
			return (List<T>) v;
		} catch (InvalidCastException) {
			return (v as JArray).ToObject<List<T>>();
		}
	}
}
