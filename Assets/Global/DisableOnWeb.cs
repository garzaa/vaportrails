using UnityEngine;

public class DisableOnWeb : MonoBehaviour {
	void OnEnable() {
		if (Application.platform.Equals(RuntimePlatform.WebGLPlayer)) {
			gameObject.SetActive(false);
		}
	}
}
