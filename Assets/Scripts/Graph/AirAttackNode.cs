using UnityEngine;
using XNode;

public class AirAttackNode : AttackNode {
    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink onLand;

    public bool singleUse;

    public override bool Enabled() {
        return base.Enabled() && !(this.graph as AttackGraph).airAttackTracker.Has(attackData.name);
    }

    public override void OnNodeEnter() {
        base.OnNodeEnter();
        if (singleUse) attackGraph.airAttackTracker.Add(attackData.name);
    }

    override public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {
        if (attackLanded && GetNode(nameof(onHit)).ConnectionCount > 0) {
            attackGraph.MoveNode(GetNode(nameof(onHit)).Connection.node as CombatNode);
        } else if (attackGraph.grounded) {
            OnGrounded();
        } else if (buffer.Ready() && (currentFrame>=attackData.IASA || attackLanded)) {
            MoveNextNode(buffer);
        } else if (clipTime >= 1) {
            attackGraph.ExitGraph();
        }
    }

    override public void OnGrounded() {
        if (GetNode("onLand").ConnectionCount>0) {
            attackGraph.MoveNode(GetNode("onLand").Connection.node as CombatNode);
        } else {
            attackGraph.ExitGraph();
        }
    }    
}
