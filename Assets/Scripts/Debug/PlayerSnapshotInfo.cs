using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSnapshotInfo : MonoBehaviour, IHitListener {
	public bool inAttack;
	public bool hitThisFrame;
	public Entity entity { get; private set; }
	public Rigidbody2D rb2d { get; private set; }
	public GroundData groundData { get; private set; }
	public EntityController controller { get; private set; }

	public void Start() {
		entity = GetComponent<Entity>();
		rb2d = entity.GetComponent<Rigidbody2D>();
		groundData = entity.GetComponent<GroundCheck>().groundData;
		controller = GetComponent<EntityController>();
	}

	public void OnHit(AttackHitbox attack) {
		hitThisFrame = true;
		this.WaitAndExecute(() => hitThisFrame = false, 1f/12f);
	}
}
