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
	protected PhysicsMaterial2D defaultMaterial;

	static GameObject jumpDust;
	static GameObject highJumpDust;
	protected static GameObject landDust;
	protected static PhysicsMaterial2D bouncyStunMaterial;
	protected static PhysicsMaterial2D frictionSlopeMaterial;
	static GameObject footfallDust;
	ParticleSystem stunSmoke;
	
	bool canGroundHitEffect = true;
	public bool staggerable = true;
	public bool takesEnvironmentDamage = true;
	[SerializeField] protected bool allowTech = true;
	[SerializeField] bool returnToSafety = true;
	public GameObject deathEffect;
	public bool destroyOnDeath = true;
	bool invincible = false;
	bool inGroundFlop = false;
	
	bool canFlip = true;

	Collider2D[] overlapResults;

	bool hitstopPriority;
    Coroutine hitstopRoutine;
	float duration;
	Vector2 hitstopExitVelocity;

	RotateToVelocity launchRotation;
	Spinner launchTumble;

	GameObject lastSafeObject;
	Vector3 lastSafeOffset;

	float fallStart = 0;
	float ySpeedLastFrame = 0;

	SafeGroundSaver groundSaver;
	
	protected const float groundFlopStunTime = 6f/12f;
	static readonly Vector2 footFallZoneCast = new Vector2(0.25f, 0.5f);

	public bool inCutscene => cutsceneSources.Count > 0;
	protected HashSet<GameObject> cutsceneSources = new HashSet<GameObject>();

	public bool overrideFootfall = false;
	public bool rigFacingRight = false;

	public bool dieOnEnviroDamage = false;

	protected virtual void Awake() {
		animator = GetComponent<Animator>();
		if (suppressAnimatorWarnings) animator.logWarnings = false;
        rb2d = GetComponent<Rigidbody2D>();
		shader = GetComponent<EntityShader>();
        groundMask = 1 << LayerMask.NameToLayer(Layers.Ground);
        collider2d = GetComponent<Collider2D>();
        groundCheck = GetComponent<GroundCheck>();
        groundData = groundCheck.groundData;
		wallData = GetComponent<WallCheck>().wallData;
		if (!jumpDust) jumpDust = Resources.Load<GameObject>("Runtime/JumpDust");
		if (!highJumpDust) highJumpDust = Resources.Load<GameObject>("Runtime/HighJumpDust");
		if (!landDust) landDust = Resources.Load<GameObject>("Runtime/LandDust");
		if (!footfallDust) footfallDust = Resources.Load<GameObject>("Runtime/FootfallDust");
		if (!bouncyStunMaterial) bouncyStunMaterial = Resources.Load<PhysicsMaterial2D>("Runtime/BounceEntity");
		if (!frictionSlopeMaterial) frictionSlopeMaterial = Resources.Load<PhysicsMaterial2D>("Runtime/FullFriction");
		defaultMaterial = rb2d.sharedMaterial;
		stunSmoke = Instantiate(Resources.Load<GameObject>("Runtime/StunSmoke"), this.transform).GetComponent<ParticleSystem>();
		stunSmoke.transform.localPosition = Vector3.zero;
		stunSmoke.Stop();
		launchRotation = GetComponentInChildren<RotateToVelocity>();
		if (launchRotation) {
			launchTumble = launchRotation.gameObject.AddComponent<Spinner>();
			launchTumble.resetOnDisable = true;
			launchTumble.rps = -1.5f;
			launchTumble.enabled = false;
		}
		groundSaver = new SafeGroundSaver(this);
		if (rigFacingRight) facingRight = true;
	}

	protected virtual void Start() {
		fallStart = transform.position.y;
		StartCoroutine(SaveLastSafePosition());
	}

    public void DoHitstop(float duration, Vector2 exitVelocity, bool priority=false, bool selfFlinch = false) {
        if (hitstopPriority && !priority) return;
		if (hitstopRoutine != null) {
			StopCoroutine(hitstopRoutine);
			hitstopRoutine = null;
		} else {
			hitstopExitVelocity = exitVelocity;
		}

		this.duration = duration;
		animator.speed = 0f;
		rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
		if (selfFlinch) shader.Flinch(exitVelocity, duration);
		hitstopRoutine = StartCoroutine(EndHitstop());
    }

    protected virtual IEnumerator EndHitstop() {
        yield return new WaitForSeconds(duration);
		rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
		if (!inCutscene) rb2d.velocity = hitstopExitVelocity;
        hitstopPriority = false;
        animator.speed = 1;
		hitstopRoutine = null;
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

	public void HighJumpDust() {
		Vector2 pos = new Vector2(
			transform.position.x,
			collider2d.bounds.min.y
		);
		GameObject g = Instantiate(highJumpDust, pos, Quaternion.identity, null);
		g.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, rb2d.velocity));
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
		Vector3 v = ForwardVector();
		v.x *= -1;
		d.transform.localScale = v;
	}

	public void SetInvincible(bool b) {
		if (b) {
			invincible = true;
			shader.StartFlashingWhite();
		} else {
			invincible = false;
			shader.StopFlashingWhite();
		}
	}

	public void CanBeHit(AttackHitbox attack) {
		if (!takesEnvironmentDamage) Debug.LogWarning(this.name + " is set to not take env damage on the entity side");
		if (invincible) return;
		if (!takesEnvironmentDamage && attack is EnvironmentHitbox) return;
	}


	public virtual void OnHit(AttackHitbox hitbox) {
		if (hitbox is EnvironmentHitbox && dieOnEnviroDamage) {
			Die();
			return;
		}
		if (staggerable) {
			Vector2 v = GetKnockback(hitbox);
			// if it's envirodamage, return to safety
			if (hitbox is EnvironmentHitbox && returnToSafety) {
				rb2d.velocity = Vector2.zero;
				CancelInvoke(nameof(ReturnToSafety));
				Invoke(nameof(ReturnToSafety), hitbox.data.hitstop);
			} else {
				// heavier people get knocked back less
				rb2d.velocity = v * (1f/rb2d.mass);
				// flip to attack
				float attackX = hitbox.transform.position.x;
				FlipTo(hitbox.gameObject);
			}

			if (hitbox.data.stunLength > 0) {
				StunFor(hitbox.data.stunLength, hitbox.data.hitstop);
				if (hitbox is EnvironmentHitbox) {
					// instant tumble for return to safety
					animator.SetBool("Tumbling", true);
				} else {
					if (!groundData.grounded) {
						animator.Update(1f);
						launchRotation?.SetAngleForVelocity(rb2d.velocity);
					}
				}
			}
			DoHitstop(hitbox.data.hitstop, rb2d.velocity, selfFlinch: true);
			shader.FlashWhite();
		} else {
			shader.FlinchOnce(GetKnockback(hitbox));
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
		CancelInvoke(nameof(UnStun));
		UnStun();
	}

	void UnStun() {
		animator.SetBool("Stunned", false);
		if (groundData.grounded) {
			animator.SetBool("Tumbling", false);
		} else {
		}
		stunned = false;
		rb2d.sharedMaterial = defaultMaterial;
		stunSmoke.Stop();
	}

	void OnCollisionEnter2D(Collision2D collision) {
		bool hitGround = Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 0.1f;
		if (stunned || animator.GetBool("Tumbling")) {
			StunImpact(hitGround);
		}
	}

	protected virtual void StunImpact(bool hitGround) {
		if ((animator.GetBool("Tumbling") || rb2d.velocity.x<5f) && hitGround) {
			GroundFlop();
		}
		else {
			StunBounce();
		}
	}

	public void LeaveTumbleAnimation() {
		animator.SetBool("Tumbling", false);
	}

	protected virtual void GroundFlop() {
		// entity will just get out of the ground flop animation half the time (but still be frozen)
		// this is caused by a transition from any state to fall? hello???
		// solution was to put a nofallinterrupt on the ground flop state
		UnStun();
		animator.SetBool("Tumbling", false);
		landNoise?.PlayFrom(gameObject);
		rb2d.velocity = Vector2.zero;
		CancelInvoke(nameof(ExecuteTech));
		Invoke(nameof(ExecuteTech), groundFlopStunTime);
		if (allowTech) animator.Play("GroundFlop", 0);
		inGroundFlop = true;
		this.WaitAndExecute(() => inGroundFlop = false, groundFlopStunTime);
	}

	void ExecuteTech() {
		if (!allowTech) return;
		inGroundFlop = false;
		if (GetComponent<EntityController>()) {
			GetComponent<EntityController>().OnTech();
		} else {
			CancelStun();
		}
		// since you can tech off the initial launch state tumbling will never be unset
		// also tumbling is set to true if stun expires in the air
		// future: differentiate between stun auto-expiring and being canceled
		animator.SetBool("Tumbling", false);
	}

	void StunBounce() {
		landNoise?.PlayFrom(gameObject);
		animator.SetBool("Tumbling", true);
	}

	protected virtual void OnEffectGroundHit(float fallDistance) {}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround && canGroundHitEffect && fallStart-transform.position.y > 4f/64f) {
			if (!stunned && defaultFootfall) {
				FootfallSound();
			}
			LandDust();
			canGroundHitEffect = false;
			this.WaitAndExecute(() => canGroundHitEffect=true, 0.1f);
			if (fallStart - transform.position.y > 7) {
				FindObjectOfType<CameraShake>().XSmallShake();
			}
			OnEffectGroundHit(fallStart - transform.position.y);
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
		// if there's not a more complex animator vwith tumbling
		if (launchRotation && !(this is EntityController)) {
			launchRotation.enabled = stunned && !groundData.grounded;
		}

		if ((ySpeedLastFrame>=0 && rb2d.velocity.y<0) || wallData.leftWall) {
			fallStart = transform.position.y;
		} 
		ySpeedLastFrame = rb2d.velocity.y;
		
		if (inGroundFlop && groundData.grounded && allowTech) {
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (!stateInfo.IsName("Base Layer.GroundFlop")) {
				animator.Play("GroundFlop", 0, 0.5f);
			}
		}

		if (launchTumble) launchTumble.enabled = animator.GetBool("Tumbling");
	}

	void RectifyEntityCollision() {
		if (!staggerable || invincible) return;
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
		if (!groundData.grounded) {
			return;
		}
		if (overrideFootfall) {
			currentFootfall = defaultFootfall;
		}
		Collider2D zone = Physics2D.OverlapBox(transform.position, footFallZoneCast, 0, Layers.FootfallZonesMask);
		if (zone != null) {
			currentFootfall = zone.GetComponent<FootfallZone>().footfallSound;
			// use this to force a groundcollider -> footstep update on zone leave
			groundColliderLastFrame = null;
			return;
		}
		
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
		(overrideFootfall ? defaultFootfall : currentFootfall).PlayFrom(gameObject);
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

	public Vector2 ForwardVector() {
		return new Vector2(
            Forward(),
            1
        ).Rotate(groundData.normalRotation);
	}

	public Vector2 BackwardVector() {
		return new Vector2(
			-Forward(),
			1
		).Rotate(groundData.normalRotation);
	}

	public int Forward() {
		return facingRight ? 1 : -1;
	}

	public void AddAttackImpulse(Vector2 impulse) {
		impulse.x *= Forward();
		rb2d.AddForce(impulse.Rotate(groundData.normalRotation), ForceMode2D.Impulse);
	}

	IEnumerator SaveLastSafePosition() {
		// space these out so there aren't lag spikes if there are a lot of entities
		yield return new WaitForSecondsRealtime(UnityEngine.Random.value);
		for (;;) {
			groundSaver.SaveIfPossible();
			yield return new WaitForSeconds(0.2f);
		}
	}

	void ReturnToSafety() {
		Vector3 lastPos = transform.position;
		transform.position = groundSaver.data.lastSafeObject.transform.position + groundSaver.data.lastSafeOffset;
		// flip so they're looking at the last position
		if (Forward() * (lastPos.x - transform.position.x) < 0) {
			_Flip();
		}
		GroundFlop();
	}

	public void Die() {
		InterruptHitstop();
		rb2d.velocity = Vector2.zero;
		if (deathEffect) {
			Instantiate(deathEffect, transform.position, Quaternion.identity);
			deathEffect.transform.parent = null;
			deathEffect.transform.localScale = ForwardVector();
		}
		if (destroyOnDeath) Destroy(gameObject);
	}

	public void EnterCutscene(GameObject source) {
		if (!gameObject.activeSelf) return;
		GetComponent<ValCombatController>()?.DisableAttackStance();
		rb2d.velocity = Vector2.zero;
		EnterCutsceneNoHalt(source);
	}

	public void EnterCutsceneNoHalt(GameObject source) {
		cutsceneSources.Add(source);
	}

	public void ExitCutscene(GameObject source) {
		if (!gameObject.activeInHierarchy) return;
		// space to continue counts as a jump input this frame
		StartCoroutine(ExitCutsceneNextFrame(source));
	}

	IEnumerator ExitCutsceneNextFrame(GameObject source) {
		yield return new WaitForEndOfFrame();
		cutsceneSources.Remove(source);
	}

	public void ForceSafetyRespawnPoint(GameObject o, Vector3 v) {
		groundSaver.ForceRespawnPoint(o, v);
	}
}
