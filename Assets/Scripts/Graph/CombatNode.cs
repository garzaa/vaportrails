using UnityEngine;
using XNode;
using System.Collections.Generic;
     
abstract public class CombatNode : Node {
    [HideInInspector]
    bool active;

    [HideInInspector]
    protected bool cancelable;

    protected AttackGraph attackGraph;

    virtual public void OnNodeEnter() {
        attackGraph = this.graph as AttackGraph;
        active = true;
        attackGraph.combatController.OnAttackNodeEnter(this);
    }

    virtual public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {

    }

    virtual public void OnNodeExit() {
        cancelable = false;
        active = false;
    }

    virtual public bool Enabled() {
        return true;
    }

    virtual public void OnGrounded() {

    }

    public bool IsActive() {
        return active;
    }

    virtual public void OnAttackLand() {
        cancelable = true;
    }
}
