using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackGraphTraverser {
	// this class is exposed to nodes
	public class Context {
		public Animator animator;
		public AttackBuffer buffer;
		public CombatController combatController;
		public PlayerInput input;
		public AttackGraphTraverser traverser;

		public float currentFrame = 0;
		public float clipTime = 0;

		public bool grounded = true;
		public bool attackLanded = false;

		float timeOffset;

		HashSet<string> airAttacksBurned = new HashSet<string>();

		public void BurnAirAttack(string attackName) {
			airAttacksBurned.Add(attackName);
		}

		public bool CanAirAttack(string attackName) {
			return !airAttacksBurned.Contains(attackName);
		}

		public void RefundAirAttacks() {
			airAttacksBurned.Clear();
		}

		public void Refresh() {
			attackLanded = false;
			RefundAirAttacks();
		}

		public void SetTimeOffset(float offset) {
			timeOffset = offset;
		}

		public float GetTimeOffset() {
			float t = timeOffset;
			timeOffset = 0;
			return t;
		}
	}

	Context context;

	public CombatNode currentNode { get; private set; }
	int currentFrame;
	bool enteredCurrentNode = false;
	float nodeSwitchTime;
	const float nodeSwitchGracePeriod = 0.1f;
	float clipLength, clipTime;

	AttackGraph currentGraph;
	GroundData groundData;

	public AttackGraphTraverser(CombatController parent) {
		context = new Context();
		context.animator = parent.GetComponent<Animator>();
		context.buffer = parent.GetComponent<AttackBuffer>();
		context.combatController = parent;
		context.input = parent.GetComponent<PlayerInput>();
		context.traverser = this;
		groundData = parent.GetComponent<GroundCheck>().groundData;
	}

	public void EnterGraph(AttackGraph graph, CombatNode entryNode = null) {
		context.Refresh();
		currentGraph = graph;
		context.animator.SetBool("Actionable", false);
		MoveNode(entryNode ?? graph.GetRootNode());
	}

	public void MoveNode(CombatNode node) {
		currentNode = node;
		enteredCurrentNode = false;
		nodeSwitchTime = Time.time;
		context.combatController.OnCombatNodeEnter(currentNode);
		context.attackLanded = false;
		node.OnNodeEnter(context);
	}
	
	public void MoveNode(CombatNode node, float timeOffset) {
		context.SetTimeOffset(timeOffset);
		MoveNode(node);
	}

	public void ExitGraph() {
		currentGraph = null;
		currentNode = null;
		context.animator.SetBool("Actionable", true);
		context.combatController.OnAttackGraphExit();
	}

	public void OnAttackLand(AttackData attack, Hurtbox hurtbox) {
		context.attackLanded = true;
	}

	public bool InGraph() {
		return currentGraph != null;
	}

	public void Update() {
		if (!InGraph()) return;
		// this will break if the animator has a blend state in an attack node
		AnimatorClipInfo[] clipInfo = context.animator.GetCurrentAnimatorClipInfo(layerIndex:0);
		if (LockState(clipInfo)) {
			clipLength = clipInfo[0].clip.length;
			clipTime = context.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			context.currentFrame = (int) ((clipTime*clipLength) * 12f);
			context.clipTime = clipTime;
			context.grounded = groundData.grounded;
			currentNode.NodeUpdate(context);
		}
	}

	bool LockState(AnimatorClipInfo[] clipInfo) {
		// this function exists because the animator doesn't instantly go to the next node
		// so only proceed if the we know it's the right state
		AnimatorStateInfo stateInfo = context.animator.GetCurrentAnimatorStateInfo(0);

		// if the current state has no animation clip in it
        // that is bad, but could be due to a weird transition issue
        // we need the clip later on so discard this frame
		if (clipInfo.Length==0) {
			return false;
		}

		bool nameCorresponds = stateInfo.IsName("Base Layer."+currentGraph.stateMachineName+"."+currentNode.GetAnimationStateName());

		if (!nameCorresponds && !enteredCurrentNode) {
            // in case something went wrong, unlock the player
            if (Time.time-nodeSwitchTime > nodeSwitchGracePeriod) {
                ExitGraph();
            }
			// wait for animator state to actually propagate
			return false;
		}

		if (nameCorresponds && !enteredCurrentNode) {
			enteredCurrentNode = true;
		}

		if (!nameCorresponds && enteredCurrentNode) {
			ExitGraph();
			return false;
		}

		return true;
	}
}
