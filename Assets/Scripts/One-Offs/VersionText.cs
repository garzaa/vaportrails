using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour {
	void Start() {
		GetComponent<Text>().text = $"PARADISE DEEPWATER EXPERIMENTAL PLATFORM\nVERSION {Application.version}b // FOR INTERNAL USE ONLY";
	}
}
