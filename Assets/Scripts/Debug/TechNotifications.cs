using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class TechNotifications : MonoBehaviour {
	public Animator early;
	public Animator miss;
	public Animator perfect;

	public UnityEvent OnPerfectFinish;

	public SpriteRenderer tallySprite;
	public Sprite[] tallyMarks;
	int perfectCount = 0;

	AudioSource perfectNoise;
	public AudioClip perfectComplete;

	const string techNotif = "TechNotification";

	void Start() {
		perfectNoise = GetComponent<AudioSource>();
	}

	public void OnEarly() {
		early.Play(techNotif);
	}

	public void OnLate() {
		miss.Play(techNotif);
	}

	public void OnPerfect() {
		perfect.Play(techNotif);
		perfectNoise.Play();
		tallySprite.sprite = tallyMarks[perfectCount];
		if (perfectCount >= 4) {
			perfectNoise.PlayOneShot(perfectComplete);
			OnPerfectFinish.Invoke();
		} else {
			perfectCount += 1;
		}
	}
}
