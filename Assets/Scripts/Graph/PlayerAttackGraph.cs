using UnityEngine;
using XNode;

[CreateAssetMenu]
public class PlayerAttackGraph : NodeGraph {
    const int attackFramerate = 16;

    float clipTime;
    float clipLength;

    int currentFrame;
	bool enteredCurrentNode = false;

    // expose these to nodes
    public Animator animator;
    public AttackBuffer buffer;
    public AirAttackTracker airAttackTracker;
    public PlayerCombatController combatController;

    CombatNode currentNode = null;
    public string exitNodeName = "Idle";

    public void Initialize(PlayerCombatController combatController, Animator anim, AttackBuffer buffer, AirAttackTracker airAttackTracker) {
        this.animator = anim;
        this.buffer = buffer;
        this.airAttackTracker = airAttackTracker;
        this.combatController = combatController;
    }

    public void EnterGraph(Node entryNode=null) {
		Debug.Log("Entering graph");
		enteredCurrentNode = false;
		animator.SetBool("Actionable", false);
        currentNode = (entryNode == null) ? GetRootNode() : entryNode as CombatNode;
        currentNode.OnNodeEnter();
		enteredCurrentNode = false;
    }

    public void ExitGraph(bool quiet=false) {
        if (currentNode != null) {
            currentNode.OnNodeExit();
        }
		animator.SetBool("Actionable", true);
		Debug.Log("Exiting graph");
		combatController.OnGraphExit();
		animator.Play("Idle");
        currentNode = null;
    }

    public void Update() {
        // assume there aren't any blend states on the animator
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex:0);

        // if the current state has no animation clip in it
		if (clipInfo.Length==0) {
			return;
		}
		string clipName = clipInfo[0].clip.name;

		bool nameCorresponds = clipName.Equals(currentNode.name);

		if (!nameCorresponds && !enteredCurrentNode) {
			// wait for animator state to actually propagate
			Debug.Log("clip "+clipName+" is not equal to node "+currentNode.name+", but not entered yet, so waiting");
			return;
		}
		
		if (nameCorresponds && !enteredCurrentNode) {
			Debug.Log("animator entered current node "+currentNode.name);
			enteredCurrentNode = true;
		}

		if (!nameCorresponds && enteredCurrentNode) {
			// otherwise we've entered the current node and it's been interrupted
			// like by falling off a ledge/getting hit or something
			Debug.Log("current node interrupted early, exiting graph");
			ExitGraph(quiet: true);
		}

		if (nameCorresponds) {
			clipLength = clipInfo[0].clip.length;
			clipTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			currentFrame = (int) ((clipTime * clipLength) * 16f);
			Debug.Log("Locked in, updating current node at frame "+currentFrame);
			currentNode.NodeUpdate(currentFrame, clipTime, buffer);
		}
    }

    public void MoveNode(CombatNode node) {
        currentNode.OnNodeExit();
        currentNode = node;
        currentNode.OnNodeEnter();
    }
    
    int GetStateHash() {
        return animator.GetCurrentAnimatorStateInfo(layerIndex:0).fullPathHash;
    }

    CombatNode GetRootNode() {
        foreach (Node i in nodes) {
            if (i is InitialBranchNode) return i as CombatNode;
        }
        return null;
    }

    public void OnAttackLand() {
        currentNode.OnAttackLand();
    }

    public void OnGroundHit() {
        if (currentNode != null) {
            currentNode.OnGroundHit();
        }
    }

}