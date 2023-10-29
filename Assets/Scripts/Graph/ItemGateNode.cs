using UnityEngine;
using XNode;
using System.Collections;
using System.Collections.Generic;

public class ItemGateNode : CombatNode {
	public Item item;

	[Input(backingValue=ShowBackingValue.Never)]
	public AttackLink input;

	[Output(backingValue=ShowBackingValue.Never)]
	public AttackLink output;

	[Output(backingValue=ShowBackingValue.Never)]
	public AttackLink outputIfNoItem;

	override public bool Enabled(AttackGraphTraverser.Context context) {
        return context.inventory.Has(item) || GetNode(nameof(outputIfNoItem)).IsConnected;
    }

	public override void OnNodeEnter(AttackGraphTraverser.Context context) {
		if (context.inventory.Has(item)) {
			context.traverser.MoveNode(GetNode("output").Connection.node as CombatNode);
		} else if (GetNode(nameof(outputIfNoItem)).IsConnected) {
			context.traverser.MoveNode(GetNode(nameof(outputIfNoItem)).Connection.node as CombatNode);
		}
	}
}
