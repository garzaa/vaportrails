using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAnimationNode : CombatNode {
	public string stateName;
	public float actionableAt = 0.9f;

	[Input(backingValue=ShowBackingValue.Never)] 
    public AttackLink input;

	public override void OnNodeEnter(AttackGraphTraverser.Context context) {
		base.OnNodeEnter(context);
		context.animator.Play(stateName, layer:0, normalizedTime:0);
	}

	override public void NodeUpdate(AttackGraphTraverser.Context context) {
		base.NodeUpdate(context);
		if (context.clipTime > actionableAt) {
			context.traverser.ExitGraph();
		}
	}

	public override string GetAnimationStateName() {
		return stateName;
	}
}
