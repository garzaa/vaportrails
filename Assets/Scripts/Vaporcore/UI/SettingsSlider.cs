using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour {
    public string prefName;
    public Text valueLabel;
    public int defaultValue = 5;
    public AudioResource changeSound;

    protected bool quiet;
    

    virtual protected void OnEnable() {
        quiet = true;
        GetComponentInChildren<Slider>().value = PlayerPrefs.GetInt(prefName, defaultValue);
        // force an update
        HandleValueChanged(GetComponentInChildren<Slider>().value);
        quiet = false;
    }

    void OnDisable() {
        OnEnable();
    }

    virtual public void HandleValueChanged(float val) {
        if (!quiet) {
            if (changeSound) changeSound.PlayFrom(gameObject);
        }
        PlayerPrefs.SetInt(prefName, (int) val);
        if (valueLabel) valueLabel.text = ((int) val).ToString();
    }
}
