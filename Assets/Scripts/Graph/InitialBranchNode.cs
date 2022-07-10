using UnityEngine;
using XNode;

public class InitialBranchNode : AttackNode {
    [Output(dynamicPortList=true, connectionType=ConnectionType.Override)]
    public AttackLink[] speedLinks;

    override public void OnNodeEnter() {
        base.OnNodeEnter();
        CombatNode next = GetNextNode(attackGraph.buffer);
        if (next != null) {
            attackGraph.MoveNode(next);
        } else {
            attackGraph.ExitGraph();
        }
    }

    override public CombatNode GetNextNode(AttackBuffer buffer) {
        CombatNode next = null;

        if (attackGraph.combatController.IsSpeeding()) {
            next = MatchAttackNode(buffer, speedLinks, portListName:"speedLinks");
        }

        if (next == null) {
            next = MatchAttackNode(buffer, links);
        }

        if (next != null) {
            Debug.Log("Found node "+next.name+", moving to it");
        } else {
            Debug.Log("found no next node");
        }

        return next;
    }
}
