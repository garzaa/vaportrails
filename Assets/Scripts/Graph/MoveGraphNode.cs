using UnityEngine;
using XNode;

public class MoveGraphNode : CombatNode {
    public CombatNode targetNode;

    [Input(backingValue=ShowBackingValue.Never)]
    public AttackLink input;

    override public void OnNodeEnter() {
        base.OnNodeEnter();
        attackGraph.combatController.EnterAttackGraph(
            targetNode.graph as AttackGraph,
            targetNode
        );
    }
}
