using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour, ISelectHandler {
    public AudioResource changeSound;
    public bool defaultValue;

    bool quiet;

	void Awake() {
		GetComponentInChildren<Toggle>().onValueChanged.AddListener(HandleValueChanged);
	}

    void OnEnable() {
        quiet = true;
        GetComponentInChildren<Toggle>().isOn = PlayerPrefs.GetInt(gameObject.name, defaultValue ? 1 : 0) == 1;
		HandleValueChanged(GetComponentInChildren<Toggle>().isOn);
        quiet = false;
    }

    public void HandleValueChanged(bool val) {
        PlayerPrefs.SetInt(this.name, val ? 1 : 0);
        if (!quiet) {
            changeSound?.PlayFrom(gameObject);
        }
		GameOptions.Load();
    }

    public void OnSelect(BaseEventData d) {
        GetComponentInParent<ScrollViewUtils>().ScrollToChild(GetComponent<RectTransform>());
    }
}

