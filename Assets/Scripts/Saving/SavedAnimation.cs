using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SavedAnimation : SavedObject {
	public AnimationClip transition;
	public AnimationClip stay;
	Animator animator;

	bool run = false;

	protected override void Initialize() {
		animator = GetComponent<Animator>();
	}

	protected override void LoadFromProperties() {
		run = Get<bool>("run");
		if (run) animator.Play(stay.name);
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["run"] = run;
	}

	public void Run() {
		animator.Play(transition.name);
		this.run = true;
	}
}
