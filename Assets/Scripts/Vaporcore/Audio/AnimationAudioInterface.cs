using UnityEngine;

public class AnimationAudioInterface : MonoBehaviour {
	public void PlayAudio(AudioResource audioResource) {
		audioResource.PlayFrom(this.gameObject);
	}

	public void PlayAudioClip(AudioClip clip) {
		GetComponent<AudioSource>().PlayOneShot(clip);
	}
}
