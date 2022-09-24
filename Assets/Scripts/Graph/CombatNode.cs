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
        active = true;
        attackGraph.combatController.OnAttackNodeEnter(this);
    }

    virtual public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {

    }

    virtual public void OnNodeExit() {
        attackLanded = false;
        active = false;
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

    public bool IsActive() {
        return active;
    }

    virtual public void OnAttackLand() {
        attackLanded = true;
    }
}
