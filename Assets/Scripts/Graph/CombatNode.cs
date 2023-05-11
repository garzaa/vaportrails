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

	public override object GetValue(NodePort port) {
        // get it as an attack link? how tf does this work
        // https://github.com/Siccity/xNode/blob/examples/Examples/MathGraph/Nodes/MathNode.cs#L15
        // if it's an output port, get its value?? huhh
        Debug.Log("Getting value for "+port.fieldName);
        // ok that's links 0 so it should work...do i have to parse it manually? dios fucking mio
        // e.g. turn "links 0" into links[0]
        return GetInputValue<AttackLink>(port.fieldName);
	}
}
