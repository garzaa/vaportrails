using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Data/AudioResource")]
public class AudioResource : ScriptableObject {
	#pragma warning disable 0649
	[SerializeField] List<AudioClip> sounds;
	[SerializeField] [Range(0, 1)] float volume = 1f;
	[SerializeField] AudioMixerGroup outputGroup;
	[SerializeField] [Range(0, 0.25f)] float pitchVariation = 0;
	#pragma warning restore 0649

	virtual public void PlayFrom(GameObject caller) {
		if (this is CompositeAudioResource && sounds.Count==0) return;

		int idx = Random.Range(0, sounds.Count);
		AudioSource callerSource = caller.GetComponent<AudioSource>();
		if (callerSource == null) {
			callerSource = caller.AddComponent<AudioSource>();
		}
		callerSource.outputAudioMixerGroup = outputGroup;
		// convert to ±pitchVariation
		if (pitchVariation != 0) callerSource.pitch = 1 + (pitchVariation * ((Random.value * 2) - 1));
		if (sounds[idx] != null) callerSource.PlayOneShot(sounds[idx], volume);
	}
}
