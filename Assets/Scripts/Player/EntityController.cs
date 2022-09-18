using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityController : Entity {
	
	#pragma warning disable 0649
	[SerializeField] GameObject playerRig;
	[SerializeField] AudioResource jumpNoise;
	#pragma warning restore 0649

	public MovementStats movement;

    const float bufferDuration = 0.1f;
	const float fModRecoveryTime = 1.5f;

	float airControlMod = 1;
	protected float fMod = 1;

	public bool frozeInputs { 
		get {
			return _frozeInputs || stunned;
		} 
		private set {
			_frozeInputs = value;
		}
	}
	private bool _frozeInputs;

	public bool inputBackwards;
	public bool inputForwards;
	public bool movingBackwards;
	public bool movingForwards;
	// dash stuff is provided but it's up to the subcontroller to implement it
	protected bool dashing;
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
	float fallStart;
	float ySpeedLastFrame;
	bool stickDownLastFrame;

	protected AttackData currentAttack;
	protected PlayerInput input;
	ToonMotion toonMotion;
	GameObject wallJumpDust;
	GameObject fastfallSpark;
	ParticleSystem speedDust;

	override protected void Awake() {
		base.Awake();
		input = GetComponent<PlayerInput>();
		toonMotion = GetComponentInChildren<ToonMotion>();
		wallJumpDust = Resources.Load<GameObject>("Runtime/WallJumpDust");
		fastfallSpark = Resources.Load<GameObject>("Runtime/FastfallSpark");
		speedDust = Resources.Load<GameObject>("Runtime/SpeedDust").GetComponentInChildren<ParticleSystem>();
		GetComponentInChildren<ToonMotion>().ignoreGameobjects.Add(speedDust.transform.parent.gameObject);
		// p = mv
		RefreshAirMovement();
		canDash = true;
	}

	override protected void Update() {
		base.Update();
		CheckFlip();
		Move();
		
		Jump();
		UpdateAnimator();
		UpdateEffects();
	}

	void FixedUpdate() {
		ApplyMovement();
	}

	void Move() {
		inputX = input.HorizontalInput();
		inputBackwards = input.HasHorizontalInput()
				&& input.HorizontalInput()*(facingRight ? 1 : -1) < 0;
		inputForwards = input.HasHorizontalInput() && !inputBackwards;
		movingBackwards = Mathf.Abs(rb2d.velocity.x) > 0.01 && rb2d.velocity.x * -transform.localScale.x < 0;
		movingForwards = input.HasHorizontalInput() && ((facingRight && rb2d.velocity.x > 0) || (!facingRight && rb2d.velocity.x < 0));
		airControlMod = Mathf.MoveTowards(airControlMod, 1, 1f * Time.deltaTime);

		// allow moving during air attacks
		if (frozeInputs && !(currentAttack!=null && !groundData.grounded)) {
			inputX = 0;
		}

		if (groundData.leftGround) {
            if (rb2d.velocity.y <= 0) {
                justWalkedOffCliff = true;
                this.WaitAndExecute(() => justWalkedOffCliff = false, bufferDuration);
            }

			// the player can initiate walltouch on the ground
			// and ground movement can override the wallflip
			if (wallData.touchingWall) {
				FlipToWall();
			}
        } else if (groundData.hitGround) {
			if (fallStart-transform.position.y > 1 && landNoise) landNoise.PlayFrom(this.gameObject);
			RefreshAirMovement();
		}

		if (wallData.leftWall) {
			justLeftWall = true;
			this.WaitAndExecute(()=>justLeftWall=false, bufferDuration*2);
		}

		if (ySpeedLastFrame>=0 && rb2d.velocity.y<0) {
			fallStart = transform.position.y;
		} 
		ySpeedLastFrame = rb2d.velocity.y;

		if (groundData.ledgeStep && !speeding && !movingForwards) {
			rb2d.velocity = new Vector2(0, rb2d.velocity.y);
		}
	}

	override protected void OnWallHit() {
		FlipToWall();
		fMod = 1;
		RefreshAirMovement();
	}

	void FlipToWall() {
		// flip to the wall
		if (facingRight && wallData.direction>0) {
			Flip();
		} else if (!facingRight && wallData.direction<0) {
			Flip();
		}
		toonMotion.ForceUpdate();
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
					rb2d.AddForce(Vector2.right * rb2d.mass * movement.airAcceleration * inputX * airControlMod);
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

		if (rb2d.velocity.y < movement.maxFallSpeed) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.maxFallSpeed);
		}

		if (wallData.touchingWall && rb2d.velocity.y < movement.maxWallSlideSpeed) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.maxWallSlideSpeed);
		}

		// fast fall
		if (!groundData.grounded
			&& !frozeInputs
			&& !wallData.touchingWall
			&& input.VerticalInput() == -1f
			&& rb2d.velocity.y<0
			&& rb2d.velocity.y > movement.maxFallSpeed*0.75f
			&& !stickDownLastFrame
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
	}



	public void DisableShortHop() {
		canShortHop = false;
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		UpdateToonMotion();
	}

	void Jump(bool executeIfBuffered=false) {
		if (stunned) return;

		if (input.ButtonUp(Buttons.JUMP) && rb2d.velocity.y > movement.shortHopCutoffVelocity && canShortHop) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, movement.shortHopCutoffVelocity);
		}

		if ((currentAttack && !currentAttack.jumpCancelable) && input.ButtonDown(Buttons.JUMP)) {
			BufferJump();
		}

		if ((!currentAttack && frozeInputs) || (currentAttack && !currentAttack.jumpCancelable)) return;

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
        }

		void WallJump() {
            bufferedJump = false;
			// assume player is facing the wall and needs to be flipped away from it
			jumpNoise.PlayFrom(this.gameObject);
			float v = movement.jumpSpeed;
			rb2d.velocity = new Vector2((facingRight ? v : -v)*1.3f, Mathf.Max(v, rb2d.velocity.y));
			airControlMod = 0;
			GameObject w = Instantiate(wallJumpDust);
			w.transform.position = new Vector2(facingRight ? collider2d.bounds.min.x : collider2d.bounds.max.x, transform.position.y);
			w.transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
			animator.SetTrigger("WallJump");
			canShortHop = false;
		}

		void AirJump() {
			if (groundData.distance<0.3f) {
				// if player is falling and about to hit ground, don't buffer an airjump
				GroundJump();
				return;
			}
			BufferJump(); // in case about to hit a wall
			currentAirJumps--;
			rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(movement.jumpSpeed, rb2d.velocity.y));
			JumpDust();
			if (movingBackwards || inputBackwards) {
				animator.SetTrigger("Backflip");
			} else {
				animator.SetTrigger("WallJump");
			}
			jumpNoise.PlayFrom(this.gameObject);
			airControlMod = 1;
			canShortHop = false;
		}

		void BufferJump() {
			bufferedJump = true;
			this.WaitAndExecute(() => bufferedJump = false, bufferDuration);
		}

		if (groundData.hitGround && bufferedJump) {
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
            } else if (wallData.touchingWall || (justLeftWall && rb2d.velocity.y<=0)) {
				WallJump();
			} else if (!wallData.touchingWall && !groundData.grounded && currentAirJumps > 0) {
				AirJump();
            }
        }
	}

	void UpdateAnimator() {
        animator.SetBool("Grounded", groundData.grounded);
        animator.SetFloat("YSpeed", rb2d.velocity.y);
        animator.SetFloat("XSpeedMagnitude", Mathf.Abs(rb2d.velocity.x));
		animator.SetBool("MovingBackward", movingBackwards);
		animator.SetBool("Wallsliding", wallData.touchingWall);
		animator.SetFloat("RelativeXSpeed", rb2d.velocity.x * Forward().x);
		animator.SetFloat("GroundDistance", groundData.distance);
		animator.SetFloat("RelativeXInput", input.HorizontalInput() * -transform.localScale.x);

        if (frozeInputs) {
			animator.SetBool("MovingForward", false);
			animator.SetFloat("XInputMagnitude", 0);
        } else {
			animator.SetBool("MovingForward", movingForwards);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(input.HorizontalInput()));
		}

		if (groundData.hitGround) {
			StartCoroutine(UpdateToonMotion());
			landingRecovery = -1;
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
		if ((inputBackwards && !movingBackwards) || !groundData.grounded || dashing || rb2d.velocity.y>0) return;
		
        if (facingRight && inputX<0) {
            Flip();
        } else if (!facingRight && inputX>0) {
            Flip();
        }
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
		frozeInputs = false;
		currentAttack = null;
		Jump(executeIfBuffered: true);
	}

	public void OnAttackNodeEnter(AttackData attackData) {
		currentAttack = attackData;
		frozeInputs = true;
		if (groundData.grounded) {
			float actualInputX = input.HorizontalInput();
			if (facingRight && actualInputX<0) {
				Flip();
			} else if (!facingRight && actualInputX>0) {
				Flip();
			}
		}
	}

	public void OnAttackNodeExit() {
	
	}

	public bool IsSpeeding() {
		// this can jitter due to fixed update stuff
		return Mathf.Abs(rb2d.velocity.x) > movement.runSpeed + 1.5f;
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
}
