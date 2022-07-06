using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

public class VolumeSlider : SettingsSlider {
    public AudioMixerGroup mixerGroup;

	// slider should be 0-10 with 5 as the default
    override public void HandleValueChanged(float val) {
        base.HandleValueChanged(val);
        // 5 should map to no change, and log(1) = 0
        val /= 5;
        mixerGroup.audioMixer.SetFloat(prefName, Mathf.Log(Mathf.Max(val, 0.0001f), 10) * 20f);
    }
}
