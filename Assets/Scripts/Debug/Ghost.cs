using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Data/Ghost")]
public class Ghost : ScriptableObject {
	public TextAsset ghostfile;
	Ghostfile ghostCache;

	void OnEnable() {
		if (ghostfile == null) return;
		ghostCache = JsonConvert.DeserializeObject<Ghostfile>(ghostfile.text);
	}

	public Ghostfile Load() {
		return ghostCache;
	}
}
