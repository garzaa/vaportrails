using System.Collections.Generic;
using System;
using UnityEngine;
using XNode;

[NodeWidth(270)]
public class AttackNode : CombatNode {
    public int IASA = 7;
	public AttackData attackData;

    [Input(backingValue=ShowBackingValue.Never)] 
    public AttackLink input;

    [Output(dynamicPortList=true, connectionType=ConnectionType.Override)]
    public AttackLink[] links;

    List<Tuple<AttackLink, CombatNode>> directionalLinks = new List<Tuple<AttackLink, CombatNode>>();
    CombatNode anyDirectionNode = null;
    Dictionary<int, Vector2> impulses = new Dictionary<int, Vector2>();

    [HideInInspector]
    public float timeOffset = 0;

    override public void NodeUpdate(int currentFrame, float clipTime, AttackBuffer buffer) {
        if (impulses.ContainsKey(currentFrame)) {
            attackGraph.rb2d.AddForce(impulses[currentFrame], ForceMode2D.Impulse);
            impulses.Remove(currentFrame);
        }

        if (attackData != null) {
            if (attackData.continuousFriction) {
                attackGraph.combatController.SetFriction(attackData.friction);
            }
        }

        if (buffer.Ready() && (cancelable || currentFrame>=IASA)) {
            if (currentFrame>=IASA) {
                MoveNextNode(buffer, allowReEntry: true);
                return;
            } else if (cancelable) {
                MoveNextNode(buffer);
                return;
            }
        }
        
        if (currentFrame>=IASA && InputManager.HasHorizontalInput()) {
            Debug.Log("iasa, exiting");
            attackGraph.ExitGraph();
            return;
        }
        
        if (clipTime >= 0.9) {
            Debug.Log("clip time > 0.9, exiting");
            attackGraph.ExitGraph();
            return;
        }
    }
 
    protected void MoveNextNode(AttackBuffer buffer, bool allowReEntry=false) {
        CombatNode next = GetNextNode(buffer);
        if (next != null) {
            attackGraph.MoveNode(next);
            return;
        }

        if (allowReEntry) {
            attackGraph.EnterGraph();
        }
    }

    virtual public CombatNode GetNextNode(AttackBuffer buffer) {
        return MatchAttackNode(buffer, this.links);
    }

    // directional attacks are prioritized in order, then the first any-directional link is used
    protected CombatNode MatchAttackNode(AttackBuffer buffer, AttackLink[] attackLinks, string portListName="links") {
        directionalLinks.Clear();
        anyDirectionNode = null;
		bool consumedAttack = false;

		// keep looking through the buffer for the next action
		while (buffer.Ready() && !consumedAttack) {
            Debug.Log("looking at the top of the current buffer");
			BufferedAttack attack = buffer.Consume();
			for (int i=0; i<attackLinks.Length; i++) {
				AttackLink link = attackLinks[i];
				if (link.type==attack.type && attack.HasDirection(link.direction)) {
                    Debug.Log("found attack, consuming attack");
					consumedAttack = true;
					CombatNode next = GetPort(portListName+" "+i).Connection.node as CombatNode;
					if (next.Enabled()) {
						if (link.direction != AttackDirection.ANY) {
							directionalLinks.Add(new Tuple<AttackLink, CombatNode>(link, next));
						} else if (anyDirectionNode == null) {
							anyDirectionNode = next;
						}
					}
				}
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

    void Awake() {
		if (attackData != null) name = attackData.name;
    }

    override public void OnNodeEnter() {
        base.OnNodeEnter();
        if (attackData != null) {
            attackGraph.animator.Play(attackData.name, layer:0, normalizedTime:timeOffset);
            foreach (AttackData.TimedImpulse ti in attackData.impulses) {
                impulses[ti.frame] = ti.impulse;
            }
            if (attackData.setFriction) {
                attackGraph.combatController.SetFriction(attackData.friction);
            }
        }
        timeOffset = 0;
    }

    override public void OnNodeExit() {
        base.OnNodeExit();
        timeOffset = 0;
        impulses.Clear();
    }
}

/*
#if UNITY_EDITOR

// highlight the current node
// unfortunately doesn't always update in time, but oh well
[CustomNodeEditor(typeof(AttackNode))]
public class AttackNodeEditor : NodeEditor {
    private AttackNode attackNode;
    private static GUIStyle editorLabelStyle;

    public override void OnBodyGUI() {
        attackNode = target as AttackNode;

        if (editorLabelStyle == null) editorLabelStyle = new GUIStyle(EditorStyles.label);
        if (attackNode.active) EditorStyles.label.normal.textColor = Color.cyan;
        base.OnBodyGUI();
        EditorStyles.label.normal = editorLabelStyle.normal;
    }
}

#endif
*/
