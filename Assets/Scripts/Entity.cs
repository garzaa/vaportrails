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
	public bool facingRight { get; private set; }
	protected GroundData groundData;
	protected WallCheckData wallData;
	protected Collider2D groundColliderLastFrame;
	public EntityShader shader { get; private set; }
	public bool stunned { get; private set; }
	
	GroundCheck groundCheck;
	AudioResource currentFootfall;
	PhysicsMaterial2D defaultMaterial;

	static GameObject jumpDust;
	protected static GameObject landDust;
	protected static PhysicsMaterial2D bouncyStunMaterial;
	static GameObject footfallDust;
	ParticleSystem stunSmoke;
	
	bool canGroundHitEffect = true;
	public bool staggerable = true;
	
	bool canFlip = true;

	Collider2D[] overlapResults;

	bool hitstopPriority;
    Coroutine hitstopRoutine;

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
		if (!bouncyStunMaterial) bouncyStunMaterial = Resources.Load<PhysicsMaterial2D>("Runtime/BounceEntity");
		defaultMaterial = rb2d.sharedMaterial;
		stunSmoke = Instantiate(Resources.Load<GameObject>("Runtime/StunSmoke"), this.transform).GetComponent<ParticleSystem>();
		stunSmoke.transform.localPosition = Vector3.zero;
		stunSmoke.Stop();
	}

    public void DoHitstop(float duration, Vector2 exitVelocity, bool priority=false) {
        if (hitstopPriority && !priority) return;
		if (hitstopRoutine != null) StopCoroutine(hitstopRoutine);
		animator.speed = 0f;
		rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
		shader.Flinch(exitVelocity, duration);
		hitstopRoutine = StartCoroutine(EndHitstop(duration, exitVelocity));
    }

    IEnumerator EndHitstop(float duration, Vector2 exitVelocity) {
        yield return new WaitForSeconds(duration);
		rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
		rb2d.velocity = exitVelocity;
        hitstopPriority = false;
        animator.speed = 1;
    }

    void InterruptHitstop() {
		if (hitstopRoutine != null) StopCoroutine(hitstopRoutine);
		hitstopPriority = false;
		animator.speed = 1f;
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
			if (attack.data.stunLength > 0) {
				StunFor(attack.data.stunLength, attack.data.hitstop);
			}
			DoHitstop(attack.data.hitstop, rb2d.velocity);
			shader.FlashWhite();
		} else {
			shader.FlinchOnce(GetKnockback(attack));
		}
	}

	public Vector2 GetKnockback(AttackHitbox attack) {
		Vector2 v = attack.data.GetKnockback(attack, this.gameObject);
		if (groundData.grounded && v.y < 0 && v.y > -5) {
			v.y = 0;
		}
		if (!attack.data.autolink) {
			float attackX = attack.transform.position.x;
			v.x *= attackX > transform.position.x ? -1 : 1;
		}
		return v;
	}

	public void StunFor(float seconds, float hitstopDuration) {
		animator.SetTrigger("OnHit");
		stunned = true;
		animator.SetBool("Stunned", true);
		animator.SetBool("Tumbling", false);
		rb2d.sharedMaterial = bouncyStunMaterial;
		stunSmoke.Play();
		CancelInvoke(nameof(UnStun));
		CancelInvoke(nameof(ExecuteTech));
		Invoke(nameof(UnStun), seconds+hitstopDuration);
	}

	public void CancelStun() {
		UnStun();
	}

	void UnStun() {
		animator.SetBool("Stunned", false);
		if (groundData.grounded) {
			animator.SetBool("Tumbling", false);
		} else {
			animator.SetBool("Tumbling", true);
		}
		stunned = false;
		rb2d.sharedMaterial = defaultMaterial;
		stunSmoke.Stop();
	}

	void OnCollisionEnter2D(Collision2D collision) {
		bool hitGround = Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 0.1f;
		if (hitGround && animator.GetBool("Tumbling")) {
			GroundFlop();
		}
		else if (stunned) {
			if (rb2d.velocity.magnitude > 1f) {
				StunBounce(collision.contacts[0].normal);
			} else {
				GroundFlop();
			}
		}
	}

	void GroundFlop() {
		landNoise?.PlayFrom(gameObject);
		rb2d.velocity = Vector2.zero;
		animator.Play("GroundFlop", 0);
		CancelInvoke(nameof(ExecuteTech));
		Invoke(nameof(ExecuteTech), 9f/12f);
	}

	void ExecuteTech() {
		if (GetComponent<EntityController>()) {
			GetComponent<EntityController>().OnTech();
		} else {
			CancelStun();
		}
	}

	void StunBounce(Vector3 collisionNormal) {
		landNoise?.PlayFrom(gameObject);
		animator.SetBool("Tumbling", true);
	}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround && canGroundHitEffect) {
			if (!stunned && defaultFootfall) {
				FootfallSound();
			}
			LandDust();
			canGroundHitEffect = false;
			this.WaitAndExecute(() => canGroundHitEffect=true, 0.1f);
		}
		if (wallData.hitWall) {
			landNoise?.PlayFrom(this.gameObject);
			GameObject g = Instantiate(landDust);
			bool wallRight = wallData.direction > 0;
			float x = wallRight ? collider2d.bounds.max.x : collider2d.bounds.min.x;
			g.transform.position = new Vector2(x, transform.position.y);
			g.transform.eulerAngles = new Vector3(0, 0, wallRight ? 90 : -90);
			OnWallHit();
		}
		RectifyEntityCollision();
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
			rb2d.AddForce(Vector3.Project((transform.position - overlapping.transform.position), Vector3.right).normalized * 4f * Time.timeScale);
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

	public void _Flip() {
		facingRight = !facingRight;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
	}

	public void FlipTo(GameObject target) {
		if (transform.position.x < target.transform.position.x && !facingRight) {
			_Flip();
		} else if (target.transform.position.x < transform.position.x && facingRight) {
			_Flip();
		}
	}

	public Vector2Int ForwardVector() {
		return new Vector2Int(
            Forward(),
            1
        );
	}

	public int Forward() {
		return facingRight ? 1 : -1;
	}

	public void AddAttackImpulse(Vector2 impulse) {
		rb2d.AddForce(impulse * ForwardVector(), ForceMode2D.Impulse);
	}
}
