using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Data/Ghost")]
public class Ghost : ScriptableObject {
	public TextAsset ghostfile;
	public float techRatio = 0.5f;

	public Ghostfile Load() {
		return JsonConvert.DeserializeObject<Ghostfile>(ghostfile.text);
	}
}
