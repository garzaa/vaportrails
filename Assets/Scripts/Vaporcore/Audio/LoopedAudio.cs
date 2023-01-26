using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopedAudio : MonoBehaviour {
	AudioSource audioSource;

	public AudioClip intro;
	public AudioClip loop;

	void Start() {
		audioSource = GetComponent<AudioSource>();
		audioSource.PlayOneShot(intro);
		audioSource.playOnAwake = false;
		audioSource.loop = true;
		audioSource.clip = loop;
		audioSource.PlayScheduled(AudioSettings.dspTime + intro.length);
	}
}
