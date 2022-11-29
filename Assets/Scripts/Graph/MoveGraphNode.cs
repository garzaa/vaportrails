using UnityEngine;
using XNode;

public class MoveGraphNode : CombatNode {
    public CombatNode targetNode;

    [Input(backingValue=ShowBackingValue.Never)]
    public AttackLink input;

    override public void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        context.traverser.EnterGraph(
            targetNode.graph as AttackGraph,
            targetNode
        );
    }
}
