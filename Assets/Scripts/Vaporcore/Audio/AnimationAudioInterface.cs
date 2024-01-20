using UnityEngine;

public class AnimationAudioInterface : MonoBehaviour {
	AudioSource audioSource;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
	}

	public void PlayAudio(AudioResource audioResource) {
		audioResource.PlayFrom(this.gameObject);
	}

	public void PlayAudioClip(AudioClip clip) {
		if (audioSource == null || clip == null) Debug.LogWarning("Audio source/clip is null for animation interface "+this.name);
		GetComponent<AudioSource>().PlayOneShot(clip);
	}
}
