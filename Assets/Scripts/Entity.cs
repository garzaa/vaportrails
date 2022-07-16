using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(WallCheck))]
[RequireComponent(typeof(GroundCheck))]
[RequireComponent(typeof(EntityShader))]
public class Entity : MonoBehaviour, IHitListener {
	#pragma warning disable 0649
	[SerializeField] AudioResource defaultFootfall;
	#pragma warning restore 0649

	protected Animator animator;
	protected Rigidbody2D rb2d;
	protected int groundMask;
	protected Collider2D collider2d;
	protected bool facingRight;
	protected GroundData groundData;
	protected WallCheckData wallData;
	protected Collider2D groundColliderLastFrame;
	protected EntityShader entityShader;
	
	GroundCheck groundCheck;
	AudioResource currentFootfall;

	static GameObject jumpDust;
	protected static GameObject landDust;
	static GameObject footfallDust;
	
	bool canGroundHitEffect = true;
	public bool staggerable = true;

	protected virtual void Awake() {
		animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
		entityShader = GetComponent<EntityShader>();
        groundMask = 1 << LayerMask.NameToLayer(Layers.Ground);
        collider2d = GetComponent<Collider2D>();
        groundCheck = GetComponent<GroundCheck>();
        groundData = groundCheck.groundData;
		wallData = GetComponent<WallCheck>().wallData;
		if (!jumpDust) jumpDust = Resources.Load<GameObject>("Runtime/JumpDust");
		if (!landDust) landDust = Resources.Load<GameObject>("Runtime/LandDust");
		if (!footfallDust) footfallDust = Resources.Load<GameObject>("Runtime/FootfallDust");
	}

	public void JumpDust() {
		Vector2 pos = new Vector2(
			transform.position.x,
			collider2d.bounds.min.y
		);
		Instantiate(jumpDust, pos, Quaternion.identity, null);
	}

	public void LandDust() {
		Vector2 pos = new Vector2(
			transform.position.x,
			collider2d.bounds.min.y
		);
		Instantiate(landDust, pos, Quaternion.identity, null);
	}

	public void FootfallDust() {
		// when transitioning into a falling animation
		if (!groundData.grounded) return;
		Vector2 pos = new Vector2(
			transform.position.x,
			collider2d.bounds.min.y
		);
		GameObject d = Instantiate(footfallDust, pos, Quaternion.identity, null);
		// keep track of facing left/right
		d.transform.localScale = transform.localScale;
	}

	public void OnHit(AttackHitbox attack) { 
		if (staggerable) {
			Vector2 v = attack.data.knockback;
			float attackX = attack.transform.position.x;
			v.x *= attackX > transform.position.x ? -1 : 1;
			// heavier people get knocked back less
			rb2d.velocity = v * (1f/rb2d.mass);
			// flip to attack
			if (facingRight && attackX<transform.position.x) {
				Flip();
			} else if (!facingRight && attackX>transform.position.x) {
				Flip();
			}
		}
	}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround && canGroundHitEffect) {
			FootfallSound();
			LandDust();
			canGroundHitEffect = false;
			WaitAndExecute(() => canGroundHitEffect=true, 0.1f);
		}
	}

	void UpdateFootfallSound() {
        if (groundData.groundCollider != groundColliderLastFrame) {
            FootfallSound s = groundData.groundCollider.GetComponent<FootfallSound>();
            if (s != null) {
                currentFootfall = s.sound;
            } else {
                currentFootfall = defaultFootfall;
            }
        }
        groundColliderLastFrame = groundData.groundCollider;
    }

	public void FootfallSound() {
		currentFootfall.PlayFrom(this.gameObject);
	}

	public void Flip() {
        facingRight = !facingRight;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
    }

	protected void WaitAndExecute(Action action, float timeout) {
        StartCoroutine(_WaitAndExecute(action, timeout));
    }

    IEnumerator _WaitAndExecute(Action action, float timeout) {
        yield return new WaitForSeconds(timeout);
        action();
    }

	public Vector2Int Forward() {
		return new Vector2Int(
            ForwardScalar(),
            1
        );
	}

	public int ForwardScalar() {
		return facingRight ? 1 : -1;
	}

	public void AddAttackImpulse(Vector2 impulse) {
		rb2d.AddForce(impulse * Forward(), ForceMode2D.Impulse);
	}
}
