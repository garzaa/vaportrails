using UnityEngine;
using XNode;

[NodeWidth(270)]
public class UnlockableGate : CombatNode {
    // public Item requiredItem;

    [Input(backingValue=ShowBackingValue.Never)]
    public AttackLink input;

    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink unlockedBranch;

    CombatNode GetUnlockedNode() {
        return (GetNode("unlockedBranch").Connection.node as CombatNode);
    }

    override public bool Enabled(AttackGraphTraverser.Context context) {
        return base.Enabled(context) 
        // && GlobalController.inventory.items.HasItem(requiredItem)
        && GetUnlockedNode().Enabled(context);
    }

    override public void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        context.traverser.MoveNode(GetUnlockedNode());
    }
}
