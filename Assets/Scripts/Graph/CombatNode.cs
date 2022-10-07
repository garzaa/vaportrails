using UnityEngine;
using XNode;
using System.Collections.Generic;
     
abstract public class CombatNode : Node {
    [HideInInspector]
    bool active;

    protected bool attackLanded;
    protected AttackGraph attackGraph;

    virtual public void OnNodeEnter() {
        attackGraph = this.graph as AttackGraph;
        attackGraph.combatController.OnCombatNodeEnter(this);
    }

    virtual public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {

    }

    virtual public string GetAnimationStateName() {
        return this.name;
    }

    virtual public void OnNodeExit() {
        attackLanded = false;
    }

    virtual public bool Enabled() {
        return true;
    }

    public bool CanMoveNode(string portName) {
        if (GetNode(portName).Connection == null) return false;
        CombatNode node = GetNode(portName).Connection.node as CombatNode;
        return node.Enabled();
    }

    virtual public void OnGrounded() {

    }

    virtual public void OnAttackLand() {
        attackLanded = true;
    }
}
