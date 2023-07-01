using UnityEngine;
using XNode;
using System.Collections.Generic;
     
abstract public class CombatNode : Node {
    public AnimationClip animationClip;

    virtual public void OnNodeEnter(AttackGraphTraverser.Context context) {}

    virtual public void NodeUpdate(AttackGraphTraverser.Context context) {}

    virtual public string GetAnimationStateName() {
        if (animationClip != null) return animationClip.name;
        return this.name;
    }

    virtual public bool Enabled(AttackGraphTraverser.Context context) {
        return true;
    }

    public bool CanMoveNode(string portName, AttackGraphTraverser.Context context) {
        if (GetNode(portName).Connection == null) return false;
        CombatNode node = GetNode(portName).Connection.node as CombatNode;
        return node.Enabled(context);
    }
}
