using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInput))]
public class EntityController : Entity {
	
	#pragma warning disable 0649
	[SerializeField] AudioResource jumpNoise;
	[SerializeField] bool faceRightOnStart;
	#pragma warning restore 0649

	public List<Ability> abilities = new List<Ability>();

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

	protected bool inputBackwards;
	protected bool inputForwards;
	protected bool movingBackwards;
	protected bool movingForwards;
	// dash stuff is provided but it's up to the subcontroller to implement it
	public bool Dashing { get; protected set; }
	public bool CanDash { get; protected set; }
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
	public bool justJumped => keepJumpSpeed;
	Coroutine keepJumpSpeedRoutine;

	public bool inAttack => currentAttack != null;
	[SerializeField] protected AttackData currentAttack;
	public AttackData GetAttack() => currentAttack;
	protected PlayerInput input;
	ToonMotion toonMotion;
	GameObject wallJumpDust;
	GameObject fastfallSpark;
	ParticleSystem speedDust;

	const float techWindow = 0.5f;
	const float techLockoutLength = 0.2f;
	bool canTech = false;
	bool techLockout = false;
	GameObject techEffect;

	// todo: this is just for training gym HUD, find more uses or take it out
	public UnityEvent TechSuccess;
	public UnityEvent TechLockout;
	public UnityEvent TechMiss;

	// slope control
	float angleLastStep = 0;
	public float angleStepDiff => groundData.normalRotation - angleLastStep;
	float jumpTime = -10;

	Rewired.Player rewiredPlayer;
	CameraShake cameraShake;
	readonly GenerousJump generousJump = new GenerousJump(); 

	override protected void Awake() {
		base.Awake();
		input = GetComponent<PlayerInput>();
		toonMotion = GetComponentInChildren<ToonMotion>();
		wallJumpDust = Resources.Load<GameObject>("Runtime/WallJumpDust");
		fastfallSpark = Resources.Load<GameObject>("Runtime/FastfallSpark");
		speedDust = Resources.Load<GameObject>("Runtime/SpeedDust").GetComponentInChildren<ParticleSystem>();
		techEffect = Resources.Load<GameObject>("Runtime/TechEffect");
		wallKickHitmarker = Resources.Load<GameObject>("Runtime/DefaultHitmarker");
		GetComponentInChildren<ToonMotion>()?.ignoreGameobjects.Add(speedDust.transform.parent.gameObject);
		// p = mv
		RefreshAirMovement();
		CanDash = true;
		if (!facingRight && faceRightOnStart) _Flip();
		rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
		cameraShake = GameObject.FindObjectOfType<CameraShake>();
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
		if (groundData.grounded && !justJumped) generousJump.StoreVelocity(rb2d);
		angleLastStep = groundData.normalRotation;
	}

	void Move() {
		inputX = input.HorizontalInput();
		inputBackwards = input.HasHorizontalInput() && input.HorizontalInput()*Forward() < 0;
		inputForwards = input.HasHorizontalInput() && !inputBackwards;
		movingBackwards = Mathf.Abs(rb2d.velocity.x) > 0.01 && rb2d.velocity.x * Forward() < 0;
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
			if (currentAirDashes == 0 && CanDash && movement.maxAirDashes > 0) {
				shader.FlashCyan();
			}
			RefreshAirMovement();
		}

		if (wallData.leftWall) {
			justLeftWall = true;
			this.WaitAndExecute(()=>justLeftWall=false, bufferDuration*2);
		}

		// stop at the end of ledges (but allow edge canceling)
		if (groundData.ledgeStep && !speeding && !(inputForwards)) {
			rb2d.velocity = new Vector2(0, rb2d.velocity.y);
		}

		if (
			wallData.touchingWall
			&& !groundData.grounded
			&& !inAttack
			&& !(stunned || animator.GetBool("Tumbling")) 
			&& animator.GetBool("WallSlideInterrupt")
		) {
			FlipToWall();
		}

		// don't slide on slopes
		if (inputX == 0 && groundData.normalRotation != 0 && rb2d.velocity.sqrMagnitude < 2f) {
			// make it a full-friction material
			rb2d.sharedMaterial = frictionSlopeMaterial;
		} else if (rb2d.sharedMaterial != bouncyStunMaterial) {
			// otherwise make it the default material
			rb2d.sharedMaterial = defaultMaterial;
		}
	}

	override protected void OnWallHit() {
		WallKick();
		// if hitting the wall from a burnt-out airdash state
		if (currentAirDashes == 0 && CanDash && movement.maxAirDashes > 0) {
			shader.FlashCyan();
		}
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
		toonMotion?.ForceUpdate();
	}

	void WallKick() {
		if (!HasAbility(Ability.Wallkick)) {
			return;
		}
		if (stunned || frozeInputs) return;
		if (wallData.collider.friction < 0.2) return;
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
		if (inCutscene) {
			if (Mathf.Abs(rb2d.velocity.x) > 0.05f) {
				SlowOnFriction();
			} else {
				rb2d.velocity = new Vector2(0, rb2d.velocity.y);
			}
			return;
		}

		speeding = Mathf.Abs(rb2d.velocity.x) > movement.runSpeed;

		void SlowOnFriction() {
            float f = groundData.grounded ? groundData.groundCollider.friction : movement.airFriction;
			// rotate the vector to ground normal, take x component and slow it, rotate back
            rb2d.velocity = rb2d.velocity.Rotate(-groundData.normalRotation);
            rb2d.velocity = new Vector2(rb2d.velocity.x * (1 - (f*f*fMod)), rb2d.velocity.y);
            rb2d.velocity = rb2d.velocity.Rotate(groundData.normalRotation);
        }

		if (Dashing) {
			float magnitude = Mathf.Max(Mathf.Abs(rb2d.velocity.x), movement.dashSpeed);
			float y = groundData.grounded ? rb2d.velocity.y : Mathf.Max(rb2d.velocity.y, 0);
			rb2d.velocity = new Vector2(magnitude * Mathf.Sign(rb2d.velocity.x), y);
			return;
		}

        if (groundData.hitGround) {
            // on a ground hit, rotate the velocity to the slope normal
            // since it wasn't being rotated the previous step
            rb2d.velocity = rb2d.velocity.Rotate(groundData.normalRotation);
		} else if (
			groundData.grounded && !justJumped
			&& (angleStepDiff != 0)
			&& (Mathf.Abs(angleStepDiff) < 225) // don't follow corners that are TOO pointy
			&& (Time.unscaledTime - jumpTime > 0.5f)
			&& ((Mathf.Abs(groundData.normalRotation) < 46f) || Mathf.Abs(groundData.normalRotation) > 90+46)
		) {
			// if moving over a convex corner of ground, then adjust velocity accordingly
			// if moving through a concave corner, physics will handle it since friction and bounce are both 0
			// counterclockwise is positive for angles!
			float vx = rb2d.velocity.x;
			bool fromUphillToFlat = (groundData.normalRotation == 0) && (
				(vx > 0 && angleStepDiff < 0) || (vx < 0 && angleStepDiff > 0)
			);
			bool fromFlatToDownhill = ((vx > 0 && groundData.normalRotation < 0) || (vx < 0 && groundData.normalRotation > 0)) && (
				(vx > 0 && angleStepDiff < 0) || (vx < 0 && angleStepDiff > 0)
			);
			if (fromUphillToFlat || fromFlatToDownhill) {
				rb2d.velocity = rb2d.velocity.Rotate(angleStepDiff);
			}
		}

        if (inputX!=0) {
			if (!speeding || (movingForwards && inputBackwards) || (movingBackwards && inputForwards)) {
				if (groundData.grounded && Vector2.Angle(Vector2.up, groundData.normal) < 46f && (Time.unscaledTime - jumpTime > 0.5f)) {
					// if ground is a platform that's been destroyed/disabled
					float f = groundData.groundCollider != null ? groundData.groundCollider.friction : movement.airFriction;
					Vector2 v = Vector2.right * rb2d.mass * movement.gndAcceleration * inputX * f*f;
					v = v.Rotate(groundData.normalRotation); 
					rb2d.AddForce(v);
				} else if (!groundData.grounded) {	
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
            } else {
				if (Mathf.Abs(rb2d.velocity.x) < 1f) {
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
		if (rb2d.velocity.y<0 && (groundData.distance)<collider2d.bounds.extents.y && groundData.platforms.Count > 0) {
			// then snap to its top
			float diff = collider2d.bounds.extents.y - groundData.distance;
			rb2d.MovePosition(rb2d.position + ((diff+0.1f) * Vector2.up) + (Vector2.right*rb2d.velocity.x*Time.fixedDeltaTime));
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

		// debounce jump inputs, but keep it buffered
		if (Time.unscaledTime < jumpTime+0.2f) {
			return;
		}

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
            if ((groundData.grounded || justWalkedOffCliff) && !keepJumpSpeed) {
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
		jumpTime = Time.time;
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
		bool wasMovingUp = rb2d.velocity.y > 0;
		rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(generousJump.GetHighestVY(rb2d), 0) + movement.jumpSpeed);
		if (IsSpeeding() && groundData.normalRotation != 0 && input.isHuman && wasMovingUp) {
			HighJumpDust();
		}
		SetJustJumped();
	}

	void WallJump() {
		if (!HasAbility(Ability.Walljump)) return;

		bufferedJump = false;
		jumpNoise.PlayFrom(this.gameObject);
		float v = movement.jumpSpeed;

		if (wallData.touchingWall && wallData.collider.friction < 0.2) {
			// if on ice, push off the wall to get in the air, but don't actually jump
			rb2d.velocity = new Vector2((-wallData.direction * movement.runSpeed)+1.5f, Mathf.Max(0, rb2d.velocity.y));
			animator.SetTrigger("WallJump");
			airControlMod = 0.0f;
		} else {
			// if inputting towards wall, jump up it
			// but always push player away from the wall
			if (wallData.direction * inputX > 0) {
				rb2d.velocity = new Vector2((-wallData.direction * movement.runSpeed), Mathf.Max(v, rb2d.velocity.y));
				// animator.SetTrigger("Backflip");
				animator.SetTrigger("WallJump");
				airControlMod = 0.2f;
			} else {
				rb2d.velocity = new Vector2((-wallData.direction * movement.runSpeed)+1.5f, Mathf.Max(v, rb2d.velocity.y));
				animator.SetTrigger("WallJump");
				airControlMod = 0.0f;
			}
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
		if (groundData.distance<0.4f && !groundData.grounded) {
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
		// currentAirDashes = movement.maxAirDashes;
		SetJustJumped();
	}

	IEnumerator KeepJumpSpeedRoutine() {
		keepJumpSpeed = true;
		groundData.jumpTime = Time.time;
		// v = v0 + at
		float timeToZero = -rb2d.velocity.y/Physics2D.gravity.y;
		yield return new WaitForSeconds(timeToZero);
		keepJumpSpeed = false;
	}

	// grace period for if you bonk your head on the ceiling
	public void SetJustJumped() {
		if (keepJumpSpeedRoutine != null) {
			StopCoroutine(keepJumpSpeedRoutine);
		}
		keepJumpSpeed = true;
		groundData.jumpTime = Time.time;
		keepJumpSpeedRoutine = StartCoroutine(KeepJumpSpeedRoutine());
	}

	void UpdateTechInputs() {
		if (input.TechInput()) {
			if (!techLockout && !canTech && stunned) {
				canTech = true;
				CancelInvoke(nameof(EndTechWindow));
				Invoke(nameof(EndTechWindow), techWindow);
			}
		}
	}

	protected override void StunImpact(bool hitGround) {
		if (!techLockout && (canTech || (!input.isHuman && Random.value < movement.techChance))) {
			TechSuccess.Invoke();
			OnTech();
			return;
		} else if (techLockout) {
			TechLockout.Invoke();
		} else if (!canTech) {
			if (input.isHuman && !PlayerInput.usingKeyboard) {
				rewiredPlayer.SetVibration(0, 1f, 0.5f);
				rewiredPlayer.SetVibration(1, 1f, 0.5f);
			}
			TechMiss.Invoke();
		}
		base.StunImpact(hitGround);
	}

	public virtual void OnTech() {
		if (!allowTech) return;
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
			shader.FlashCyan();
		} else if (groundData.grounded) {
			rb2d.velocity = new Vector2(
				input.HasHorizontalInput() ? movement.runSpeed * Mathf.Sign(input.HorizontalInput()) : 0,
				0
			);
			Instantiate(
				techEffect,
				transform.position + Vector3.down*collider2d.bounds.extents.y,
				Quaternion.identity,
				null
			);
			shader.FlashCyan();
		}
		CancelInvoke(nameof(UnfreezeInputs));
		UnfreezeInputs();
		UpdateAnimator();
		if (input.HasHorizontalInput() && groundData.grounded) {
			animator.SetTrigger("TechSuccess");
		} else {
			animator.Play("Idle", 0);
		}
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

	protected virtual void UpdateAnimator() {
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
			animator.SetFloat("RelativeXInput", input.HorizontalInput() * Forward());
			animator.SetBool("MovingForward", movingForwards);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(input.HorizontalInput()));
		}

		if (groundData.hitGround) {
			StartCoroutine(UpdateToonMotion());
			landingRecovery = -1;
		}

		landingRecovery = Mathf.MoveTowards(landingRecovery, 0, 4f * Time.deltaTime);
		animator.SetFloat("LandingRecovery", landingRecovery);
    }
	
	IEnumerator UpdateToonMotion() {
		yield return new WaitForEndOfFrame();
		toonMotion?.ForceUpdate();
	}

	void UpdateEffects() {
		ParticleSystem.EmissionModule emission = speedDust.emission;
		emission.rateOverDistance = IsSpeeding() ? 1.5f : 0;
	}

	void CheckFlip() {
		if ((inputBackwards && !movingBackwards) || !groundData.grounded || Dashing || frozeInputs) return;

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
		CanDash = false;
		Dashing = true;
		fMod = 0;
		if (input.isHuman) cameraShake.Shake(Vector2.right * Forward() * 0.05f);

		// dash at the max direction indicated by the stick, additive even if backwards
		float speed = movement.runSpeed+movement.dashSpeed;
		speed = Mathf.Max(Mathf.Abs(rb2d.velocity.x)+movement.dashSpeed, speed);

		Vector2 v = new Vector2(speed * Mathf.Sign(input.HorizontalInput()), 0).Rotate(groundData.normalRotation);
		if (!groundData.grounded) v.y = Mathf.Max(rb2d.velocity.y, 0);

		rb2d.velocity = v;
		if (!groundData.grounded) currentAirDashes--;
		// called here because sometimes a dash can happen between physics steps
		UpdateLastVelocity();

		this.WaitAndExecute(EndDashCooldown, movement.dashCooldown);
	}

	protected override void OnEffectGroundHit(float fallDistance) {
		// vibrate a bit on both motors
		if (fallDistance > 7f) {
			cameraShake.Shake(Vector2.up * 0.5f);
			if (!PlayerInput.usingKeyboard) {
				rewiredPlayer.SetVibration(0, 1f, 0.5f);
				rewiredPlayer.SetVibration(1, 1f, 0.5f);
			}
		}
		else if (fallDistance > 2f) {
			if (!PlayerInput.usingKeyboard) {
				rewiredPlayer.SetVibration(0, 0.5f, 0.2f);
				rewiredPlayer.SetVibration(1, 0.5f, 0.2f);
			}
		}
	}

	public override void OnHit(AttackHitbox hitbox) {
		base.OnHit(hitbox);
		if (hitbox is EnvironmentHitbox) {
			if (input.isHuman) cameraShake.Shake(Vector2.up * 0.5f);
			if (!PlayerInput.usingKeyboard) {
				rewiredPlayer.SetVibration(0, 1f, 0.5f);
				rewiredPlayer.SetVibration(1, 1f, 0.5f);
			}
		}
	}

	public void StopDashAnimation() {
		fMod = 0;
		Dashing = false;
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
		if (Dashing) StopDashAnimation();
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
		if ((groundData.grounded || allowFlip) && (attackData && !attackNode.FromBackwardsInput())) {
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
		if (CanDash) return;
		// don't flash cyan if it's an enemy not being controlled
		if (input.isHuman && (groundData.grounded || wallData.touchingWall || (currentAirDashes>0))) {
			shader.FlashCyan();
		}
		CanDash = true;
	}

	public void AddAbility(Ability a) {
		abilities.Add(a);
	}

	public void RemoveAbility(Ability a) {
		abilities.Remove(a);
	}

	public bool HasAbility(Ability a) {
		return abilities.Contains(a);
	}

	public void LoadAbilities(List<Ability> a) {
		this.abilities.Clear();
		this.abilities.AddRange(a);
	}

	public List<Ability> GetAbilities() {
		return this.abilities;
	}

#if UNITY_EDITOR
	[ContextMenu("Add Basic Animator Params")]
	public void AddBasicAnimatorParams() {
		RuntimeAnimatorController c = GetComponent<Animator>().runtimeAnimatorController;
		var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(c));
		controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
		controller.AddParameter("Tumbling", AnimatorControllerParameterType.Bool);
		controller.AddParameter("Stunned", AnimatorControllerParameterType.Bool);
		controller.AddParameter("XSpeedMagnitude", AnimatorControllerParameterType.Float);
		controller.AddParameter("YSpeed", AnimatorControllerParameterType.Float);
		controller.AddParameter("LandingRecovery", AnimatorControllerParameterType.Float);
		controller.AddParameter("OnHit", AnimatorControllerParameterType.Trigger);
		controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
		controller.AddParameter("Actionable", AnimatorControllerParameterType.Bool);
		controller.AddParameter("LandingLag", AnimatorControllerParameterType.Bool);
		controller.AddParameter("XInputMagnitude", AnimatorControllerParameterType.Float);
	}
#endif
}
