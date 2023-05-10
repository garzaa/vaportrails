using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EntityController : Entity {
	
	#pragma warning disable 0649
	[SerializeField] GameObject playerRig;
	[SerializeField] AudioResource jumpNoise;
	[SerializeField] bool faceRightOnStart;
	#pragma warning restore 0649

	HashSet <Ability> abilities = new HashSet<Ability>();

	public MovementStats movement;

    const float bufferDuration = 0.1f;
	const float fModRecoveryTime = 1.5f;

	float airControlMod = 1;
	protected float fMod = 1;

	public bool frozeInputs { 
		get {
			return _frozeInputs || stunned || inCutscene;
		} 
		private set {
			_frozeInputs = value;
		}
	}
	[SerializeField] private bool _frozeInputs;

	public bool inCutscene => cutsceneSources.Count > 0;

	protected bool inputBackwards;
	protected bool inputForwards;
	protected bool movingBackwards;
	protected bool movingForwards;
	// dash stuff is provided but it's up to the subcontroller to implement it
	public bool dashing { get; protected set; }
	public bool canDash { get; protected set; }
	protected int currentAirJumps;
	protected int currentAirDashes;

	bool justWalkedOffCliff;
	bool justLeftWall;
	bool bufferedJump;
	bool speeding;
	bool canShortHop;
	float inputX;
	float landingRecovery = 1;
	float storedSpeed;
	float wallKickBoost = 0.25f;
	GameObject wallKickHitmarker;

	bool stickDownLastFrame;
	bool keepJumpSpeed;
	Coroutine keepJumpSpeedRoutine;

	public bool inAttack => currentAttack != null;
	[SerializeField] protected AttackData currentAttack;
	public AttackData GetAttack() => currentAttack;
	protected PlayerInput input;
	ToonMotion toonMotion;
	GameObject wallJumpDust;
	GameObject fastfallSpark;
	ParticleSystem speedDust;

	HashSet<MonoBehaviour> cutsceneSources = new HashSet<MonoBehaviour>();

	const float techWindow = 0.3f;
	const float techLockoutLength = 0.6f;
	bool canTech = false;
	bool techLockout = false;
	GameObject techEffect;

	// todo: this is just for training gym HUD, find more uses or take it out
	public UnityEvent TechSuccess;
	public UnityEvent TechLockout;
	public UnityEvent TechMiss;

	override protected void Awake() {
		base.Awake();
		input = GetComponent<PlayerInput>();
		toonMotion = GetComponentInChildren<ToonMotion>();
		wallJumpDust = Resources.Load<GameObject>("Runtime/WallJumpDust");
		fastfallSpark = Resources.Load<GameObject>("Runtime/FastfallSpark");
		speedDust = Resources.Load<GameObject>("Runtime/SpeedDust").GetComponentInChildren<ParticleSystem>();
		techEffect = Resources.Load<GameObject>("Runtime/TechEffect");
		wallKickHitmarker = Resources.Load<GameObject>("Runtime/DefaultHitmarker");
		GetComponentInChildren<ToonMotion>().ignoreGameobjects.Add(speedDust.transform.parent.gameObject);
		// p = mv
		RefreshAirMovement();
		canDash = true;
		if (!facingRight && faceRightOnStart) _Flip();
	}

	override protected void Update() {
		base.Update();
		CheckFlip();
		Move();
		Jump();
		UpdateAnimator();
		UpdateEffects();
		UpdateTechInputs();
	}

	void UpdateLastVelocity() {
		if (Mathf.Abs(rb2d.velocity.x) > 1) storedSpeed = Mathf.Abs(rb2d.velocity.x);
	}

	void FixedUpdate() {
		ApplyMovement();
		UpdateLastVelocity();
	}

	void Move() {
		inputX = input.HorizontalInput();
		inputBackwards = input.HasHorizontalInput() && input.HorizontalInput()*Forward() < 0;
		inputForwards = input.HasHorizontalInput() && !inputBackwards;
		movingBackwards = Mathf.Abs(rb2d.velocity.x) > 0.01 && rb2d.velocity.x * -transform.localScale.x < 0;
		movingForwards = input.HasHorizontalInput() && ((facingRight && rb2d.velocity.x > 0) || (!facingRight && rb2d.velocity.x < 0));
		airControlMod = Mathf.MoveTowards(airControlMod, 1, 0.5f * Time.deltaTime);

		// allow moving during air attacks
		if (frozeInputs && !(currentAttack!=null && !groundData.grounded)) {
			inputX = 0;
		}

		if (groundData.leftGround) {
            if (rb2d.velocity.y <= 0) {
                justWalkedOffCliff = true;
                this.WaitAndExecute(() => justWalkedOffCliff = false, bufferDuration);
            }
        } else if (groundData.hitGround) {
			RefreshAirMovement();
		}

		if (wallData.leftWall) {
			justLeftWall = true;
			this.WaitAndExecute(()=>justLeftWall=false, bufferDuration*2);
		}

		// stop at the end of ledges (but allow edge canceling)
		if (groundData.ledgeStep && !speeding && !input.HasHorizontalInput()) {
			rb2d.velocity = new Vector2(0, rb2d.velocity.y);
		}

		if (
			wallData.touchingWall
			&& !groundData.grounded
			&& !inAttack
			&& !(stunned || animator.GetBool("Tumbling")) 
		) {
			FlipToWall();
		}
	}

	override protected void OnWallHit() {
		WallKick();
		fMod = 1;
		RefreshAirMovement();
	}

	void FlipToWall() {
		if (inAttack) return;
		if (facingRight && wallData.direction>0) {
			Flip();
		} else if (!facingRight && wallData.direction<0) {
			Flip();
		}
		toonMotion.ForceUpdate();
	}

	void WallKick() {
		// hack for not giving a wallkick ability yet
		if (!input.isHuman) return;
		if (stunned || frozeInputs) return;
		if (WasSpeeding() && rb2d.velocity.y > 0.1) {
			DisableFlip();
			if (wallData.direction * Forward() < 0) {
				_Flip();
			}
			animator.Play("WallKick");
			jumpNoise.PlayFrom(this.gameObject);
			bufferedJump = false;
			shader.FlashWhite();
			float preCollisionSpeed = storedSpeed;
			rb2d.velocity = new Vector2(
				0,
				movement.jumpSpeed + (preCollisionSpeed * wallKickBoost)
			);
			GameObject w = Instantiate(wallJumpDust);
			w.transform.position = new Vector2(wallData.direction > 0 ? collider2d.bounds.min.x : collider2d.bounds.max.x, transform.position.y);
			w.transform.localScale = new Vector3(wallData.direction > 0 ? 1 : -1, 1, 1);
			Instantiate(wallKickHitmarker, w.transform.position, Quaternion.identity);
			canShortHop = false;
			SetJustJumped();
		}
	}

	void ApplyMovement() {
		speeding = Mathf.Abs(rb2d.velocity.x) > movement.runSpeed;

		void SlowOnFriction() {
            float f = groundData.grounded ? groundData.groundCollider.friction : movement.airFriction;
            rb2d.velocity = new Vector2(rb2d.velocity.x * (1 - (f*f*fMod)), rb2d.velocity.y);
        }

		if (dashing) {
			float magnitude = Mathf.Max(Mathf.Abs(rb2d.velocity.x), movement.dashSpeed);
			rb2d.velocity = new Vector2(magnitude * Mathf.Sign(rb2d.velocity.x), Mathf.Max(rb2d.velocity.y, 0));
			return;
		}

        if (inputX!=0) {
			if (!speeding || (movingForwards && inputBackwards) || (movingBackwards && inputForwards)) {
				if (groundData.grounded) {
					// if ground is a platform that's been destroyed/disabled
					float f = groundData.groundCollider != null ? groundData.groundCollider.friction : movement.airFriction;
					rb2d.AddForce(Vector2.right * rb2d.mass * movement.gndAcceleration * inputX * f*f);
				} else {	
					float attackMod = inAttack ? 0.5f : 1f;
					rb2d.AddForce(Vector2.right * rb2d.mass * movement.airAcceleration * inputX * airControlMod * attackMod);
				}
			}
        } else {
            // if no input, slow player
            if (groundData.grounded) {
                if (Mathf.Abs(rb2d.velocity.x) > 0.05f) {
                    SlowOnFriction();
                } else {
                    rb2d.velocity = new Vector2(0, rb2d.velocity.y);
                }
            }
        }

        if (speeding) {
            SlowOnFriction();
        }

		if (rb2d.velocity.y < movement.maxFallSpeed && !inAttack) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.maxFallSpeed);
		}

		if (wallData.touchingWall && rb2d.velocity.y < movement.maxWallSlideSpeed) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.maxWallSlideSpeed);
		}

		// fastfall
		if (!groundData.grounded
			&& !stunned
			&& !wallData.touchingWall
			&& input.VerticalInput() == -1f
			&& !stickDownLastFrame
			&& rb2d.velocity.y<=0
			&& rb2d.velocity.y > movement.maxFallSpeed*0.75f
			&& inAttack
		) {
			Vector3 offset = ((Vector3) Random.insideUnitCircle + Vector3.down) * 0.5f;
			Instantiate(fastfallSpark, transform.position + offset, Quaternion.identity);
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.maxFallSpeed * 0.75f);
		}

		if (!frozeInputs) {
			stickDownLastFrame = input.LeftStick().y == -1;
		} else {
			stickDownLastFrame = false;
		}

		if (!stunned) {
			fMod = Mathf.MoveTowards(fMod, 1, (1f/fModRecoveryTime) * Time.fixedDeltaTime);
		} else {
			fMod = 0.1f;
		}

		// if falling down through a platform (ground distance above feet)
		// that distance can vary due to physics and/or float precision
		if (rb2d.velocity.y<0 && (groundData.distance)<collider2d.bounds.extents.y) {
			// then snap to its top
			float diff = collider2d.bounds.extents.y - groundData.distance;
			rb2d.MovePosition(rb2d.position + ((diff+0.1f) * Vector2.up));
			// cancel downward velocity
			rb2d.velocity = new Vector2(
				rb2d.velocity.x,
				0.1f
			);
		}
	}

	public void DisableShortHop() {
		canShortHop = false;
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		StartCoroutine(UpdateToonMotion());
	}

	void Jump(bool executeIfBuffered=false) {
		if (stunned) return;

		if (input.ButtonUp(Buttons.JUMP) && rb2d.velocity.y > movement.shortHopCutoffVelocity && canShortHop) {
			keepJumpSpeed = false;
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.shortHopCutoffVelocity);
		}

		// allow jump-canceling moves if player is touching the wall
		bool onWall = !groundData.grounded && wallData.touchingWall;

		if (currentAttack && !onWall) {
			if ((!currentAttack.moveCancelable) && input.ButtonDown(Buttons.JUMP)) {
				BufferJump();
			}
		}

		if ((!currentAttack && frozeInputs) || (currentAttack && !currentAttack.moveCancelable && !onWall)) return;

		if (keepJumpSpeed && !frozeInputs) {
			rb2d.velocity = new Vector2(
				rb2d.velocity.x,
				Mathf.Max(0, rb2d.velocity.y)
			);
		}

		if (groundData.hitGround && bufferedJump && !keepJumpSpeed) {
			// if they didn't just jump (and the groundcheck hasn't had time to update)
            GroundJump();
            return;
        } else if (wallData.hitWall && bufferedJump) {
			WallJump();
			return;
		}

		if (input.ButtonDown(Buttons.JUMP) || (executeIfBuffered && bufferedJump)) {
            if (groundData.grounded || justWalkedOffCliff) {
                if (groundData.platforms.Count > 0 && Input.GetAxisRaw("Vertical") < -0.8f) {
					DropThroughPlatforms(groundData.platforms);
					return;
				}
                GroundJump();
            } else if (wallData.touchingWall || (justLeftWall && rb2d.velocity.y<=0 && !IsSpeeding())) {
				WallJump();
			} else if (!wallData.touchingWall && !groundData.grounded && currentAirJumps > 0) {
				AirJump();
            }
        }
	}

	void BufferJump() {
		bufferedJump = true;
		this.WaitAndExecute(() => bufferedJump = false, bufferDuration);
	}

	void GroundJump() {
		bufferedJump = false;
		jumpNoise.PlayFrom(this.gameObject);
		if (!wallData.touchingWall) {
			if ((movingForwards && inputBackwards) || (movingBackwards && !inputForwards)) {
				animator.SetTrigger("Backflip");
			} else {
				animator.SetTrigger("Jump");
			}
		}
		canShortHop = true;
		JumpDust();
		rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(movement.jumpSpeed, rb2d.velocity.y));
		SetJustJumped();
	}

	void WallJump() {
		if (!HasAbility(Ability.Walljump)) return;

		bufferedJump = false;
		jumpNoise.PlayFrom(this.gameObject);
		float v = movement.jumpSpeed;
		// if inputting towards wall, jump up it
		// but always push player away from the wall
		if (wallData.direction * inputX > 0) {
			rb2d.velocity = new Vector2((-wallData.direction * movement.runSpeed), Mathf.Max(v, rb2d.velocity.y));
			animator.SetTrigger("Backflip");
			airControlMod = 0.2f;
		} else {
			rb2d.velocity = new Vector2((-wallData.direction * movement.runSpeed)+1.5f, Mathf.Max(v, rb2d.velocity.y));
			animator.SetTrigger("WallJump");
			airControlMod = 0.0f;
		}
		// flip away from the wall
		if (wallData.direction * Forward() > 0) {
			_Flip();
		}
		GameObject w = Instantiate(wallJumpDust);
		w.transform.position = new Vector2(wallData.direction > 0 ? collider2d.bounds.min.x : collider2d.bounds.max.x, transform.position.y);
		w.transform.localScale = new Vector3(wallData.direction > 0 ? 1 : -1, 1, 1);
		canShortHop = false;
		SetJustJumped();
	}

	void AirJump() {
		if (groundData.distance<0.2f) {
			// if player is falling and about to hit ground, don't buffer an airjump
			GroundJump();
			return;
		}
		BufferJump(); // in case about to hit a wall

		if (!HasAbility(Ability.Airjump)) return;

		currentAirJumps--;
		rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(movement.jumpSpeed, rb2d.velocity.y));
		JumpDust();
		if (movingBackwards || inputBackwards) {
			animator.SetTrigger("Backflip");
		} else {
			animator.SetTrigger("AirJump");
		}
		jumpNoise.PlayFrom(this.gameObject);
		airControlMod = 1;
		canShortHop = false;
		currentAirDashes = movement.maxAirDashes;
		SetJustJumped();
	}

	IEnumerator KeepJumpSpeedRoutine() {
		keepJumpSpeed = true;
		// v = v0 + at
		float timeToZero = -movement.jumpSpeed/Physics2D.gravity.y;
		yield return new WaitForSeconds(timeToZero);
		keepJumpSpeed = false;
	}

	// grace period for if you bonk your head on the ceiling
	public void SetJustJumped() {
		if (keepJumpSpeedRoutine != null) {
			StopCoroutine(keepJumpSpeedRoutine);
		}
		keepJumpSpeed = true;
		keepJumpSpeedRoutine = StartCoroutine(KeepJumpSpeedRoutine());
	}

	void UpdateTechInputs() {
		if (input.ButtonDown(Buttons.SPECIAL) || input.ButtonDown(Buttons.PARRY)) {
			if (!techLockout && !canTech && stunned) {
				canTech = true;
				CancelInvoke(nameof(EndTechWindow));
				Invoke(nameof(EndTechWindow), techWindow);
			}
		}
		
		if ((stunned || animator.GetBool("Tumbling")) && (groundData.hitGround || wallData.hitWall)) {

		}
	}

	protected override void StunImpact(bool hitGround) {
		if (!techLockout && canTech) {
			TechSuccess.Invoke();
			OnTech();
			return;
		} else if (techLockout) {
			TechLockout.Invoke();
		} else if (!canTech) {
			TechMiss.Invoke();
		}
		base.StunImpact(hitGround);
	}

	public virtual void OnTech() {
		CancelStun();
		animator.SetBool("Tumbling", false);
		if (wallData.touchingWall) {
			rb2d.velocity = Vector2.zero;
			RefreshAirMovement();
			Instantiate(
				techEffect,
				transform.position + new Vector3(wallData.direction * collider2d.bounds.extents.x, 0, 0),
				Quaternion.identity,
				null
			);
		} else {
			rb2d.velocity = new Vector2(
				movement.runSpeed * Mathf.Sign(input.HorizontalInput()),
				0
			);
			Instantiate(
				techEffect,
				transform.position + Vector3.down*collider2d.bounds.extents.y,
				Quaternion.identity,
				null
			);
		}
		CancelInvoke(nameof(UnfreezeInputs));
		UnfreezeInputs();
		UpdateAnimator();
		if (input.HasHorizontalInput() && groundData.grounded) {
			animator.SetTrigger("TechSuccess");
		} else {
			animator.Play("Idle", 0);
		}
		shader.FlashCyan();
		canTech = false;
		CancelInvoke(nameof(EndTechWindow));
		GetComponent<CombatController>()?.OnTech();
		// freeze inputs for a sec while teching ON GROUND
		if (groundData.grounded) {
			FreezeInputs();
			Invoke(nameof(UnfreezeInputs), 0.5f);
		}
		SetInvincible(true);
		this.WaitAndExecute(() => SetInvincible(false), 0.5f);
	}

	void EndTechWindow() {
		canTech = false;
		techLockout = true;
		this.WaitAndExecute(() => techLockout = false, techLockoutLength);
	}

	protected override void GroundFlop() {
		base.GroundFlop();
		FreezeInputs();
		CancelInvoke(nameof(UnfreezeInputs));
		Invoke(nameof(UnfreezeInputs), groundFlopStunTime);
	}

	void UpdateAnimator() {
        animator.SetBool("Grounded", groundData.grounded);
        animator.SetFloat("YSpeed", rb2d.velocity.y);
        animator.SetFloat("XSpeedMagnitude", Mathf.Abs(rb2d.velocity.x));
		animator.SetBool("MovingBackward", movingBackwards);
		// edge-raycasts can hit the wall
		animator.SetBool("Wallsliding", wallData.touchingWall && groundData.distance > 0.3f);
		animator.SetFloat("RelativeXSpeed", rb2d.velocity.x * ForwardVector().x);
		animator.SetFloat("GroundDistance", groundData.distance);

        if (frozeInputs) {
			animator.SetBool("MovingForward", false);
			animator.SetFloat("XInputMagnitude", 0);
			animator.SetFloat("RelativeXInput", 0);
        } else {
			animator.SetFloat("RelativeXInput", input.HorizontalInput() * -transform.localScale.x);
			animator.SetBool("MovingForward", movingForwards);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(input.HorizontalInput()));
		}

		if (groundData.hitGround) {
			StartCoroutine(UpdateToonMotion());
			landingRecovery = -1;
			// TODO: move this to ValController or something
			// also only make it happen if fall distance is more than 0.2 because subway
			// oh and platform bumps now, this is real
			HairForwards();
		}

		landingRecovery = Mathf.MoveTowards(landingRecovery, 0, 4f * Time.deltaTime);
		animator.SetFloat("LandingRecovery", landingRecovery);
    }
	
	IEnumerator UpdateToonMotion() {
		yield return new WaitForEndOfFrame();
		toonMotion.ForceUpdate();
	}

	void UpdateEffects() {
		ParticleSystem.EmissionModule emission = speedDust.emission;
		emission.rateOverDistance = IsSpeeding() ? 1.5f : 0;
	}

	void CheckFlip() {
		if ((inputBackwards && !movingBackwards) || !groundData.grounded || dashing || frozeInputs) return;

        if (facingRight && inputX<0) {
            Flip();
        } else if (!facingRight && inputX>0) {
            Flip();
        }
    }

	public void DashIfPossible(AudioResource dashSound) {
		if (!groundData.grounded && currentAirDashes <= 0) return;

		dashSound.PlayFrom(gameObject);
		animator.SetTrigger(inputBackwards ? "BackDash" : "Dash");
		canDash = false;
		dashing = true;
		fMod = 0;
		// dash at the max direction indicated by the stick
		// if already moving in that way, make it additive
		float speed = movement.runSpeed+movement.dashSpeed;
		if ((inputForwards && movingForwards) || (inputBackwards && movingBackwards)) {
			speed = Mathf.Max(Mathf.Abs(rb2d.velocity.x)+movement.dashSpeed, speed);
		}
		rb2d.velocity = new Vector2(
			speed * Mathf.Sign(input.HorizontalInput()),
			Mathf.Max(rb2d.velocity.y, 0)
		);
		if (!groundData.grounded) currentAirDashes--;
		// called here because sometimes a dash can happen between physics steps
		UpdateLastVelocity();

		this.WaitAndExecute(EndDashCooldown, movement.dashCooldown);
	}

	public void StopDashAnimation() {
		fMod = 0;
		dashing = false;
	}

	void DropThroughPlatforms(List<RaycastHit2D>platforms) {
		foreach (RaycastHit2D hit in platforms) {
			EdgeCollider2D platform = hit.collider.GetComponent<EdgeCollider2D>();
			if (platform == null) continue;
			platform.enabled = false;
            this.WaitAndExecute(() => platform.enabled = true, 0.5f);
		}
	}

	public virtual void OnAttackGraphEnter() {
		if (dashing) StopDashAnimation();
	}

	public void OnAttackGraphExit() {
		UnfreezeInputs();
		currentAttack = null;
		Jump(executeIfBuffered: true);
	}

	public void OnAttackNodeEnter(AttackNode attackNode) {
		AttackData attackData = attackNode?.attackData;
		currentAttack = attackData;
		FreezeInputs();
		// bespoke for divekick animation
		bool allowFlip = (attackNode is AirAttackNode && (attackNode as AirAttackNode).allowFlip);
		if ((groundData.grounded || allowFlip) && (attackData && !attackData.fromBackwardsInput)) {
			float actualInputX = input.HorizontalInput();
			if (facingRight && actualInputX<0) {
				Flip();
			} else if (!facingRight && actualInputX>0) {
				Flip();
			}
		}
	}

	public bool IsSpeeding() {
		// this can jitter due to fixed update stuff
		return Mathf.Abs(rb2d.velocity.x) > movement.runSpeed + 1.5f;
	}

	bool WasSpeeding() {
		return storedSpeed >= movement.runSpeed + 1.5f;
	}

	public void SetFmod(float f) {
		fMod = f;
	}

	public void OnLedgePop() {
        RefreshAirMovement();
    }

	public void RefreshAirMovement() {
		currentAirDashes = movement.maxAirDashes;
		currentAirJumps = movement.maxAirJumps;
	}

	public void HairBackwards() {
		animator.SetTrigger("HairBackwards");
	}

	public void HairForwards() {
		animator.SetTrigger("HairForwards");
	}

	public void FreezeInputs() {
		frozeInputs = true;
	}

	public void UnfreezeInputs() {
		frozeInputs = false;
	}

	public bool HasAirJumps() {
		return currentAirJumps > 0;
	}

	public void EndDashCooldown() {
		if (canDash) return;
		// don't flash cyan if it's an enemy not being controlled
		if (input.isHuman) {
			shader.FlashCyan();	
		}
		canDash = true;
	}

	public void EnterCutscene(MonoBehaviour source) {
		GetComponent<ValCombatController>()?.DisableAttackStance();
		rb2d.velocity = Vector2.zero;
		animator.Play("Idle", 0);
		EnterCutsceneNoHalt(source);
	}

	public void EnterCutsceneNoHalt(MonoBehaviour source) {
		cutsceneSources.Add(source);
	}

	public void ExitCutscene(MonoBehaviour source) {
		// space to continue counts as a jump input this frame
		StartCoroutine(ExitCutsceneNextFrame(source));
	}

	IEnumerator ExitCutsceneNextFrame(MonoBehaviour source) {
		yield return new WaitForEndOfFrame();
		cutsceneSources.Remove(source);
	}

	public void AddAbility(Ability a) {
		abilities.Add(a);
	}

	public bool HasAbility(Ability a) {
		return true;
		// TODO: actually check for items when back from vacation
		// return abilities.Contains(a);
	}
}
