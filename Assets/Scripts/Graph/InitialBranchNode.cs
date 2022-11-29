using UnityEngine;
using XNode;

public class InitialBranchNode : AttackNode {
    [Output(dynamicPortList=true, connectionType=ConnectionType.Override)]
    public AttackLink[] speedLinks;

    override public void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        CombatNode next = GetNextNode(context);
        if (next != null) {
            context.traverser.MoveNode(next);
        } else {
            context.traverser.ExitGraph();
        }
    }

    override public CombatNode GetNextNode(AttackGraphTraverser.Context context) {
        CombatNode next = null;

        if (context.combatController.IsSpeeding() && speedLinks.Length > 0) {
            next = MatchAttackNode(context, speedLinks, portListName:nameof(speedLinks));
        }

        if (next == null) {
            next = MatchAttackNode(context, links);
        }

        return next;
    }
}
