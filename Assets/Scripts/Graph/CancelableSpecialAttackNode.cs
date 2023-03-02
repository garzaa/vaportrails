using UnityEngine;
using XNode;

public class CancelableSpecialAttackNode : AirAttackNode {
	[Output(backingValue = ShowBackingValue.Always)]
    public AttackLink instantCancel = null;
	// this doesn't consume the buffer btw

    override public void NodeUpdate(AttackGraphTraverser.Context context) {
        base.NodeUpdate(context);
		if (
			GetNode(nameof(instantCancel)).IsConnected
			&& context.buffer.Ready()
			&& AttackBuffer.Match(context.buffer.Peek(), instantCancel)
		) {
			context.buffer.Consume();
            context.traverser.MoveNode(GetNode(nameof(instantCancel)).Connection.node as CombatNode);
        }
    }
}
