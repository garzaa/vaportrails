using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO: make basic animation node and attack node inherit from this
// they both inherit clip time and play animation states
public abstract class AnimationStateNode : CombatNode {
	public string stateName;

	[Input(backingValue=ShowBackingValue.Never)] 
    public AttackLink input;

	abstract public string GetStateName();

	public override void OnNodeEnter() {
		base.OnNodeEnter();
		attackGraph.animator.Play(stateName, layer:0, normalizedTime:0);
	}

	override public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {
		base.NodeUpdate(currentFrame, clipTime, buffer);
		// if (clipTime > actionableAt) {
		// 	Debug.Log("exiting graph, animation finished");
		// 	attackGraph.ExitGraph();
		// }
	}
}
