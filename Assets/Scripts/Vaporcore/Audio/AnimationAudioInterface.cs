using UnityEngine;

public class AnimationAudioInterface : MonoBehaviour {
	public void PlayAudio(AudioResource audioResource) {
		audioResource.PlayFrom(this.gameObject);
	}
}
