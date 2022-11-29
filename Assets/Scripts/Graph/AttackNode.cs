using System.Collections.Generic;
using System;
using UnityEngine;
using XNode;

[NodeWidth(270)]
public class AttackNode : CombatNode {
	public AttackData attackData;

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
        string portListName=nameof(links),
        bool loopOnce=false
    ) {
        directionalLinks.Clear();
        anyDirectionNode = null;
		bool foundCandidate = false;

		// keep looking through the buffer for the next action
		while (context.buffer.Ready() && !foundCandidate) {
			BufferedAttack attack = context.buffer.Consume();
			for (int i=0; i<attackLinks.Length; i++) {
				AttackLink link = attackLinks[i];
				if (link.type==attack.type && attack.HasDirection(link.direction)) {
					foundCandidate = true;
					CombatNode next = GetNode(portListName+" "+i).Connection.node as CombatNode;
					if (next.Enabled(context)) {
						if (link.direction != AttackDirection.ANY) {
							directionalLinks.Add(new Tuple<AttackLink, CombatNode>(link, next));
						} else if (anyDirectionNode == null) {
							anyDirectionNode = next;
						}
					}
                    break;
				}
			}
            if (loopOnce && !foundCandidate) {
                context.buffer.Refund(attack);
                return null;
            }
		}

        if (directionalLinks.Count > 0) {
            return directionalLinks[0].Item2;
        }

        if (anyDirectionNode != null) {
            return anyDirectionNode;
        }

        return null;
    }
}
