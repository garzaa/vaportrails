using UnityEngine;
using XNode;

public class AirAttackNode : AttackNode {
    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink onLand;

    public bool singleUse;

    public override bool Enabled() {
        return base.Enabled() && !(this.graph as PlayerAttackGraph).airAttackTracker.Has(attackData.name);
    }

    public override void OnNodeEnter() {
        base.OnNodeEnter();
        if (singleUse) attackGraph.airAttackTracker.Add(attackData.name);
    }

    override public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {
        if (buffer.Ready() && (currentFrame>=IASA || cancelable)) {
            MoveNextNode(buffer);
        } else if (clipTime >= 1) {
            attackGraph.ExitGraph();
        }
    }

    override public void OnGroundHit() {
        if (GetPort("onLand").ConnectionCount>0) attackGraph.MoveNode(GetPort("onLand").Connection.node as CombatNode);
    }    
}
