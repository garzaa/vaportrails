using System.Collections.Generic;
using System;
using UnityEngine;
using XNode;

[NodeWidth(270)]
public class AttackNode : CombatNode {
	public AttackData attackData;
    [SerializeField] bool fromBackwardsInput;

    [Input(backingValue=ShowBackingValue.Never)] 
    public AttackLink input;

    [Output(dynamicPortList=true, connectionType=ConnectionType.Override)]
    public AttackLink[] links;

    List<Tuple<AttackLink, CombatNode>> directionalLinks = new List<Tuple<AttackLink, CombatNode>>();
    CombatNode anyDirectionNode = null;

    [Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
    public AttackLink onHit;

    override public void OnNodeEnter(AttackGraphTraverser.Context context) {
        base.OnNodeEnter(context);
        if (FromBackwardsInput()) {
        }
        if (!string.IsNullOrEmpty(GetAnimationStateName())) {
            context.animator.Play(GetAnimationStateName(), layer:0, normalizedTime:context.GetTimeOffset());
        }
    }

    override public string GetAnimationStateName() {
        if (attackData) return attackData.name;
        return "";
    }

    override public void NodeUpdate(AttackGraphTraverser.Context context) {
        base.NodeUpdate(context);
        if (context.attackLanded && CanMoveNode(nameof(onHit), context)) {
            CombatNode node = GetNode(nameof(onHit)).Connection.node as CombatNode;
            context.traverser.MoveNode(node);
            return;
        } else if (context.buffer.Ready() && (context.attackLanded || context.currentFrame>=attackData.IASA)) {
            MoveNextNode(context);
        }

        context.animator.SetBool("Actionable", context.currentFrame>=attackData.IASA);
    }
 
    protected void MoveNextNode(AttackGraphTraverser.Context context) {
        CombatNode next = GetNextNode(context);
        if (next != null) {
            context.traverser.MoveNode(next);
        }
    }

    virtual public CombatNode GetNextNode(AttackGraphTraverser.Context context) {
        return MatchAttackNode(context, this.links);
    }

    // directional attacks are prioritized in order, then the first any-directional link is used
    protected CombatNode MatchAttackNode(
        AttackGraphTraverser.Context context,
        AttackLink[] attackLinks,
        string portListName=nameof(links)
    ) {
        directionalLinks.Clear();
        anyDirectionNode = null;

        if (!context.buffer.Ready()) return null;

        // match the current action to the buffer if it exists
        BufferedAttack attack = context.buffer.Peek();
        for (int i=0; i<attackLinks.Length; i++) {
            AttackLink link = attackLinks[i];
            if (AttackBuffer.Match(attack, link)) {
                CombatNode next = GetNode(portListName+" "+i).Connection.node as CombatNode;
                if (next.Enabled(context)) {
                    if (link.direction != AttackDirection.ANY) {
                        directionalLinks.Add(new Tuple<AttackLink, CombatNode>(link, next));
                    } else if (anyDirectionNode == null) {
                        anyDirectionNode = next;
                    }
                    break;
                }
            }
        }

        if (directionalLinks.Count > 0) {
            context.buffer.Consume();
            return directionalLinks[0].Item2;
        }

        if (anyDirectionNode != null) {
            context.buffer.Consume();
            return anyDirectionNode;
        }

        return null;
    }

    public bool FromBackwardsInput() {
        return fromBackwardsInput;
    }
}
