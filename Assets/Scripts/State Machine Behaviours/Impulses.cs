using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Impulses : StateMachineBehaviour {
	[System.Serializable]
	public class TimedImpulse {
		public int frame;
		public Vector2 impulse;
	}

	[Tooltip("1 frame = 1/12 of a second")]
	public List<TimedImpulse> frameImpulses;

	Dictionary<int, Vector2> impulses = new Dictionary<int, Vector2>();
	Entity entity;

	void Awake() {
		foreach (TimedImpulse ti in frameImpulses) {
			impulses[ti.frame] = ti.impulse;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!entity) entity = animator.GetComponent<Entity>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		float clipLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
		int currentFrame = (int) ((stateInfo.normalizedTime * clipLength) * 12f);
		if (impulses.ContainsKey(currentFrame)) {
			entity.AddAttackImpulse(impulses[currentFrame]);
			impulses.Remove(currentFrame);
		}
	}
}
