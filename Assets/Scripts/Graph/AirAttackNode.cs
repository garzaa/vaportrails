using UnityEngine;
using XNode;

public class AirAttackNode : AttackNode {
    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink onLand;

    public bool allowFlip;
    public bool singleUse;
    public int landingLagFrames = 2;

    public override bool Enabled(AttackGraphTraverser.Context context) {
        return base.Enabled(context) && context.CanAirAttack(name);
    }

    public override void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        if (singleUse) context.BurnAirAttack(attackData.name);
    }

    override public void NodeUpdate(AttackGraphTraverser.Context context) {
        if (context.grounded) {
            OnGrounded(context);
        } else if (context.attackLanded && CanMoveNode(nameof(onHit), context)) {
            context.traverser.MoveNode(GetNode(nameof(onHit)).Connection.node as CombatNode);
        } else if (context.buffer.Ready() && (context.currentFrame>=attackData.IASA || context.attackLanded)) {
            MoveNextNode(context);
        } else if (context.clipTime>=1) {
            context.traverser.ExitGraph();
        }
    }

    void OnGrounded(AttackGraphTraverser.Context context) {
        if (CanMoveNode(nameof(onLand), context)) {
            context.traverser.MoveNode(GetNode("onLand").Connection.node as CombatNode);
        } else {
            context.traverser.ExitGraph();
            if (landingLagFrames > 0) context.combatController.LandingLag(landingLagFrames/12f);
        }
    }    
}
