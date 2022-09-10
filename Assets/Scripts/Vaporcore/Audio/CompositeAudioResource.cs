using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Data/Composite AudioResource")]
public class CompositeAudioResource : AudioResource {
	#pragma warning disable 0649
	[SerializeField] List<AudioResource> compositeResources;
	#pragma warning restore 0649

	override public void PlayFrom(GameObject caller) {
		base.PlayFrom(caller);
		foreach (AudioResource resource in compositeResources){
			resource.PlayFrom(caller);
		}
	}
}
