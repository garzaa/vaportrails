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
	[SerializeField] protected AudioResource landNoise;
	[SerializeField] bool suppressAnimatorWarnings = false;
	#pragma warning restore 0649

	protected Animator animator;
	protected Rigidbody2D rb2d;
	protected int groundMask;
	protected Collider2D collider2d;
	protected bool facingRight;
	protected GroundData groundData;
	protected WallCheckData wallData;
	protected Collider2D groundColliderLastFrame;
	public EntityShader shader { get; private set; }
	public bool stunned { get; private set; }
	
	GroundCheck groundCheck;
	AudioResource currentFootfall;
	PhysicsMaterial2D defaultMaterial;
	RotateToVelocity stunRotation;
	Spinner stunSpin;

	static GameObject jumpDust;
	protected static GameObject landDust;
	protected static PhysicsMaterial2D stunMaterial;
	static GameObject footfallDust;
	ParticleSystem stunSmoke;
	
	bool canGroundHitEffect = true;
	public bool staggerable = true;
	bool stunBounced;
	
	bool canFlip = true;

	Collider2D[] overlapResults;

	protected virtual void Awake() {
		animator = GetComponent<Animator>();
		if (suppressAnimatorWarnings) animator.logWarnings = false;
        rb2d = GetComponent<Rigidbody2D>();
		rb2d.interpolation = RigidbodyInterpolation2D.Extrapolate;
		shader = GetComponent<EntityShader>();
        groundMask = 1 << LayerMask.NameToLayer(Layers.Ground);
        collider2d = GetComponent<Collider2D>();
        groundCheck = GetComponent<GroundCheck>();
        groundData = groundCheck.groundData;
		wallData = GetComponent<WallCheck>().wallData;
		if (!jumpDust) jumpDust = Resources.Load<GameObject>("Runtime/JumpDust");
		if (!landDust) landDust = Resources.Load<GameObject>("Runtime/LandDust");
		if (!footfallDust) footfallDust = Resources.Load<GameObject>("Runtime/FootfallDust");
		if (!stunMaterial) stunMaterial = Resources.Load<PhysicsMaterial2D>("Runtime/BounceEntity");
		defaultMaterial = rb2d.sharedMaterial;
		stunSmoke = Instantiate(Resources.Load<GameObject>("Runtime/StunSmoke"), this.transform).GetComponent<ParticleSystem>();
		stunSmoke.transform.localPosition = Vector3.zero;
		stunSmoke.Stop();
		stunRotation = GetComponentInChildren<RotateToVelocity>();
		if (stunRotation) {
			stunSpin = stunRotation.gameObject.AddComponent<Spinner>();
			stunSpin.rps = -1.5f;
			stunSpin.enabled = false;
		}
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
			// TODO: alter knockback vector based on hurtbox point distance from line of attack center + knockback vector
			// https://answers.unity.com/questions/263308/projection-of-a-point-on-a-line.html
			Vector2 v = GetKnockback(attack);
			// heavier people get knocked back less
			rb2d.velocity = v * (1f/rb2d.mass);
			// flip to attack
			float attackX = attack.transform.position.x;
			if (facingRight && attackX<transform.position.x) {
				Flip();
			} else if (!facingRight && attackX>transform.position.x) {
				Flip();
			}
			if (attack.data.stunLength > 0) StunFor(attack.data.stunLength);
			shader.FlashWhite();
			shader.Flinch(GetKnockback(attack) * new Vector2(-1, 1), attack.data.hitstop);
		} else {
			shader.FlinchOnce(GetKnockback(attack) * new Vector2(-1, 1));
		}
	}

	public Vector2 GetKnockback(AttackHitbox attack) {
		Vector2 v = attack.data.GetKnockback();
		float attackX = attack.transform.position.x;
		v.x *= attackX > transform.position.x ? -1 : 1;
		return v;
	}

	void StunFor(float seconds) {
		CancelStun();
		stunBounced = false;
		stunned = true;
		animator.SetBool("Stunned", true);
		rb2d.sharedMaterial = stunMaterial;
		stunSmoke.Play();
		Invoke(nameof(UnStun), seconds);
	}

	public void CancelStun() {
		if (IsInvoking(nameof(UnStun))) {
			CancelInvoke(nameof(UnStun));
		}
		UnStun();
	}

	void UnStun() {
		animator.SetBool("Stunned", false);
		animator.SetBool("Tumbling", false);
		rb2d.sharedMaterial = defaultMaterial;
		stunned = false;
		stunBounced = false;
		stunSmoke.Stop();
	}

	void StunBounce() {
		if (stunBounced) {
			CancelStun();
			return;
		}
		animator.SetBool("Tumbling", true);
		stunBounced = true;
		this.WaitAndExecute(() => rb2d.sharedMaterial = defaultMaterial, 0.1f);
		if (stunSpin) stunSpin.enabled = true;
	}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround && canGroundHitEffect) {
			if (!stunned) {
				if (defaultFootfall) FootfallSound();
			} else {
				landNoise?.PlayFrom(this.gameObject);
			}
			LandDust();
			canGroundHitEffect = false;
			this.WaitAndExecute(() => canGroundHitEffect=true, 0.1f);
			if (stunned && !wallData.touchingWall) StunBounce();
		}
		if (wallData.hitWall) {
			landNoise?.PlayFrom(this.gameObject);
			GameObject g = Instantiate(landDust);
			bool wallRight = wallData.direction > 0;
			float x = wallRight ? collider2d.bounds.max.x : collider2d.bounds.min.x;
			g.transform.position = new Vector2(x, transform.position.y);
			g.transform.eulerAngles = new Vector3(0, 0, wallRight ? 90 : -90);
			OnWallHit();
			if (stunned && !groundData.grounded) StunBounce();
		}
		RectifyEntityCollision();
		if (stunRotation) {
			stunRotation.enabled = stunned && !groundData.grounded && !stunBounced;
			stunSpin.enabled = stunned && !groundData.grounded && stunBounced;
		}
	}

	void RectifyEntityCollision() {
		// push self away if standing on top of someone
		if (stunned) return;
		overlapResults = Physics2D.OverlapBoxAll(
			transform.position,
			collider2d.bounds.size / 2f,
			0,
			Layers.EnemyCollidersMask | Layers.PlayerMask
		);
		Collider2D overlapping = null;
		for (int i=0; i<overlapResults.Length; i++) {
			if (overlapResults[i] != collider2d) {
				overlapping = overlapResults[i];
				break;
			}
		}
		if (overlapping) {
			Entity entity = overlapping.GetComponent<Entity>();
			if (entity && entity.stunned) return;
			rb2d.AddForce(Vector3.Project((transform.position - overlapping.transform.position), Vector3.right).normalized * 4f);
		}
	}

	protected virtual void OnWallHit() {

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

	public void DisableFlip() {
		canFlip = false;
	}

	public void EnableFlip() {
		canFlip = true;
	}

	public void Flip() {
        if (!canFlip) return;
		_Flip();
    }

	void _Flip() {
		facingRight = !facingRight;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
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
