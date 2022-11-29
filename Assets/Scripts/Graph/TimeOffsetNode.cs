using UnityEngine;
using XNode;

public class TimeOffsetNode : CombatNode {
    public float normalizedOffset;

    [Input(backingValue=ShowBackingValue.Never)]
    public AttackLink input;

    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink output;

    override public void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        AttackNode next = GetNode("output").Connection.node as AttackNode;
        context.traverser.MoveNode(next, normalizedOffset);
    }
}
