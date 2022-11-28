using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubwayCar : MonoBehaviour {
	public AudioClip doorClose;
	public AudioClip doorOpen;
	Animator anim;

	AudioSource audioSource;
	public AudioSource[] doorNoise;

	EventOnInteract[] doorInteracts;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		doorInteracts = GetComponentsInChildren<EventOnInteract>();
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
		foreach (EventOnInteract d in doorInteracts) {
			d.GetComponent<BoxCollider2D>().enabled = true;
		}
	}

	public void DisableBoarding() {
		foreach (EventOnInteract d in doorInteracts) {
			d.GetComponent<BoxCollider2D>().enabled = false;
		}
	}
}
