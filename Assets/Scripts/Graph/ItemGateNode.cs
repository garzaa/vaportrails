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

	override public bool Enabled(AttackGraphTraverser.Context context) {
        return context.inventory.Has(item);
    }

	public override void OnNodeEnter(AttackGraphTraverser.Context context) {
		context.traverser.MoveNode(GetNode("output").Connection.node as CombatNode);
	}
}
