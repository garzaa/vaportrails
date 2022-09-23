using UnityEngine;
using XNode;

[CreateAssetMenu]
public class AttackGraph : NodeGraph {
    const int attackFramerate = 16;

    float clipTime;
    float clipLength;

    int currentFrame;
	bool enteredCurrentNode = false;

    // expose these to nodes
    public Animator animator;
    public AttackBuffer buffer;
    public AirAttackTracker airAttackTracker;
    public CombatController combatController;
    public bool grounded;
    public PlayerInput inputManager;

    CombatNode currentNode = null;

    float nodeSwitchTime;
    const float nodeSwitchGracePeriod = 0.1f;

    public string stateMachineName = "GroundAttacks";

    public void Initialize(CombatController combatController, Animator anim, AttackBuffer buffer, AirAttackTracker airAttackTracker, PlayerInput inputManager) {
        this.animator = anim;
        this.buffer = buffer;
        this.airAttackTracker = airAttackTracker;
        this.combatController = combatController;
        this.inputManager = inputManager;
    }

    public void EnterGraph(CombatNode entryNode=null) {
		enteredCurrentNode = false;
		animator.SetBool("Actionable", false);
        currentNode = (entryNode == null) ? GetRootNode() : entryNode;
        currentNode.OnNodeEnter();
		enteredCurrentNode = false;
        nodeSwitchTime = Time.unscaledTime;
    }

    public void ExitGraph() {
        if (currentNode != null) {
            currentNode.OnNodeExit();
        }
		animator.SetBool("Actionable", true);
		combatController.OnGraphExit();
        currentNode = null;
    }

    public void Update() {
        // assume there aren't any blend states on the animator
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex:0);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // if the current state has no animation clip in it
        // that is bad, but could be due to a weird transition issue
        // we need the clip later on so discard this frame
		if (clipInfo.Length==0) {
			return;
		}

        bool nameCorresponds = stateInfo.IsName("Base Layer."+stateMachineName+"."+currentNode.name);

		if (!nameCorresponds && !enteredCurrentNode) {
            // in case something went wrong, unlock the player
            if (Time.unscaledTime-nodeSwitchTime > nodeSwitchGracePeriod) {
                ExitGraph();
            }
			// wait for animator state to actually propagate
			return;
		}
		
		if (nameCorresponds && !enteredCurrentNode) {
			enteredCurrentNode = true;
		}

		if (!nameCorresponds && enteredCurrentNode) {
			ExitGraph();
		}

		if (nameCorresponds) {
			clipLength = clipInfo[0].clip.length;
			clipTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			currentFrame = (int) ((clipTime * clipLength) * 12f);
			currentNode.NodeUpdate(currentFrame, clipTime, buffer);
		}
    }

    public void MoveNode(CombatNode node) {
        currentNode.OnNodeExit();
        currentNode = node;
        currentNode.OnNodeEnter();
        nodeSwitchTime = Time.unscaledTime;
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

    public void UpdateGrounded(bool g) {
        grounded = g;
    }

}
