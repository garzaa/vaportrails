using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubwayCar : MonoBehaviour {
	public AudioClip doorClose;
	public AudioClip doorOpen;
	Animator anim;

	AudioSource audioSource;
	public AudioSource[] doorNoise;

	void Start() {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
	}

	public void CloseDoors() {
		foreach (AudioSource audio in doorNoise) {
			audio.PlayOneShot(doorClose);
		}
		anim.SetBool("DoorsOpen", false);
	}

	public void OpenDoors() {
		foreach (AudioSource audio in doorNoise) {
			audio.PlayOneShot(doorOpen);
		}
		anim.SetBool("DoorsOpen", true);
	}

	public void EnableBoarding() {
		// enable the door interactables
	}

	public void DisableBoarding() {
		// disable the door interactables
	}
}
