using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAnimationNode : CombatNode {
	public string stateName;
	public float actionableAt = 0.9f;

	[Input(backingValue=ShowBackingValue.Never)] 
    public AttackLink input;

	public override void OnNodeEnter() {
		base.OnNodeEnter();
		Debug.Log("entering state "+stateName);
		attackGraph.animator.Play(stateName, layer:0, normalizedTime:0);
	}

	override public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {
		base.NodeUpdate(currentFrame, clipTime, buffer);
		if (clipTime > actionableAt) {
			Debug.Log("exiting graph, animation finished");
			attackGraph.ExitGraph();
		}
	}
}
