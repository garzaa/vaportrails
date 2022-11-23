using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubwayCar : MonoBehaviour {
	public AudioClip doorClose;
	public AudioClip doorOpen;
	Animator anim;

	public void CloseDoors() {
		anim.SetBool("DoorsOpen", false);
	}

	public void OpenDoors() {
		anim.SetBool("DoorsOpen", true);
	}
}
