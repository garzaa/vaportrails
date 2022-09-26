using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSnapshotInfo : MonoBehaviour, IHitListener, IAttackLandListener {
	public bool inAttack;
	public bool hitThisFrame;
	public bool attackLanded;
	public Entity entity { get; private set; }
	public Rigidbody2D rb2d { get; private set; }
	public GroundData groundData { get; private set; }
	public EntityController controller { get; private set; }
	public WallCheckData wallCheck { get; private set; }
	Animator animator;

	public void Start() {
		entity = GetComponent<Entity>();
		rb2d = entity.GetComponent<Rigidbody2D>();
		groundData = entity.GetComponent<GroundCheck>().groundData;
		controller = GetComponent<EntityController>();
		wallCheck = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
	}

	public void OnHit(AttackHitbox attack) {
		hitThisFrame = true;
		this.WaitAndExecute(() => hitThisFrame = false, 1f/12f);
	}

	public void OnAttackLand(AttackData data, Hurtbox hurtbox) {
		attackLanded = true;
		this.WaitAndExecute(() => attackLanded = false, 8f/12f);
	}

	public byte GetShortState() {
		return ((byte)animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
	}
}
