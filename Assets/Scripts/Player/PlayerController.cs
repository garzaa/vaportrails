using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Entity, IAttackLandListener {

	#pragma warning disable 0649
	[SerializeField] GameObject playerRig;
	[SerializeField] AudioResource jumpNoise;
	[SerializeField] ParticleSystem speedDust;
	#pragma warning restore 0649

	const float runSpeed = 4.5f;
    const float groundAcceleration = 175;
    const float airAcceleration = 80;
    const float jumpCutoffVelocity = 2f;
    const float jumpForce = 8;
    const float airFriction = 0.3f;
    const float slideFrictionMod = 0.05f;
    const float bufferDuration = 0.1f;
	const float maxFallSpeed = -14f;
	const float maxWallSlideSpeed = -4f;
	const float dashCooldown = 0.6f;
	const float dashSpeed = 6f;
	float airControlMod = 1;
	float fMod = 1;
	float fModRecoveryTime = 1.5f;

	public bool frozeInputs { 
		get {
			return _frozeInputs || stunned;
		} 
		private set {
			_frozeInputs = value;
		}
	}
	private bool _frozeInputs;

	bool inputBackwards;
	bool inputForwards;
	bool movingBackwards;
	bool justWalkedOffCliff;
	bool justLeftWall;
	bool bufferedJump;
	bool movingForwards;
	bool speeding;
	bool canDash = true;
	bool dashing;
	bool canShortHop;
	float inputX;
	float landingRecovery = 1;
	int airDashes = 1;
	int airJumps = 1;
	public float jumpSpeed { get; private set; }
	float fallStart;
	float ySpeedLastFrame;
	bool stickDownLastFrame;

	ToonMotion toonMotion;
	GameObject wallJumpDust;
	AudioResource dashSound;
	AttackData currentAttack;
	HP hp;
	PlayerInput input;
	GameObject fastfallSpark;

	override protected void Awake() {
		base.Awake();
		input = GetComponent<PlayerInput>();
		toonMotion = GetComponentInChildren<ToonMotion>();
		wallJumpDust = Resources.Load<GameObject>("Runtime/WallJumpDust");
		dashSound = Resources.Load<AudioResource>("Runtime/DashSound");
		fastfallSpark = Resources.Load<GameObject>("Runtime/FastfallSpark");
		// p = mv
		jumpSpeed = jumpForce / rb2d.mass;
	}

	override protected void Update() {
		base.Update();
		CheckFlip();
		Move();
		Dash();
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
			if (fallStart-transform.position.y > 1) landNoise.PlayFrom(this.gameObject);
			RefreshAirMovement();
		}

		if (wallData.hitWall) {
			OnWallHit();
		} else if (wallData.leftWall) {
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
		speeding = Mathf.Abs(rb2d.velocity.x) > runSpeed;

		void SlowOnFriction() {
            float f = groundData.grounded ? groundData.groundCollider.friction : airFriction;
            rb2d.velocity = new Vector2(rb2d.velocity.x * (1 - (f*f*fMod)), rb2d.velocity.y);
        }

		if (dashing) {
			float magnitude = Mathf.Max(Mathf.Abs(rb2d.velocity.x), dashSpeed);
			rb2d.velocity = new Vector2(magnitude * Mathf.Sign(rb2d.velocity.x), Mathf.Max(rb2d.velocity.y, 0));
			return;
		}

        if (inputX!=0) {
			if (!speeding || (movingForwards && inputBackwards) || (movingBackwards && inputForwards)) {
				if (groundData.grounded) {
					// if ground is a platform that's been destroyed/disabled
					float f = groundData.groundCollider != null ? groundData.groundCollider.friction : airFriction;
					rb2d.AddForce(Vector2.right * rb2d.mass * groundAcceleration * inputX * f*f);
				} else {	
					rb2d.AddForce(Vector2.right * rb2d.mass * airAcceleration * inputX * airControlMod);
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

		if (rb2d.velocity.y < maxFallSpeed) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, maxFallSpeed);
		}

		if (wallData.touchingWall && rb2d.velocity.y < maxWallSlideSpeed) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, maxWallSlideSpeed);
		}

		// fast fall
		if (!groundData.grounded
			&& !frozeInputs
			&& !wallData.touchingWall
			&& input.VerticalInput() == -1f
			&& rb2d.velocity.y<0
			&& rb2d.velocity.y > maxFallSpeed*0.75f
			&& !stickDownLastFrame
		) {
			Vector3 offset = ((Vector3) Random.insideUnitCircle + Vector3.down) * 0.5f;
			Instantiate(fastfallSpark, transform.position + offset, Quaternion.identity);
			rb2d.velocity = new Vector2(rb2d.velocity.x, maxFallSpeed * 0.75f);
		}

		if (!frozeInputs) {
			stickDownLastFrame = input.LeftStick().y == -1;
		} else {
			stickDownLastFrame = false;
		}

		fMod = Mathf.MoveTowards(fMod, 1, (1f/fModRecoveryTime) * Time.fixedDeltaTime);
	}

	void Dash() {
		void EndDashCooldown() {
			if (canDash) return;
			entityShader.FlashCyan();
			canDash = true;
		}

		if (frozeInputs && !currentAttack) return;

		if (input.ButtonDown(Buttons.SPECIAL) && canDash && input.HasHorizontalInput() && input.VerticalInput()<0.5) {
			if (!groundData.grounded && airDashes <= 0) return;
			dashSound.PlayFrom(gameObject);
			animator.SetTrigger(inputBackwards ? "BackDash" : "Dash");
			entityShader.FlashWhite();
			canDash = false;
			dashing = true;
			fMod = 0;
			// dash at the max direction indicated by the stick
			// if already moving in that way, make it additive
			float speed = runSpeed+dashSpeed;
			if ((inputForwards && movingForwards) || (inputBackwards && movingBackwards)) {
				speed = Mathf.Max(Mathf.Abs(rb2d.velocity.x)+dashSpeed, speed);
			}
			rb2d.velocity = new Vector2(
				speed * Mathf.Sign(input.HorizontalInput()),
				Mathf.Max(rb2d.velocity.y, 0)
			);
			if (!groundData.grounded) airDashes--;
			this.WaitAndExecute(EndDashCooldown, dashCooldown);
		}
	}

	public void StopDashAnimation() {
		fMod = 0;
		dashing = false;
	}

	public void DisableShortHop() {
		canShortHop = false;
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		UpdateToonMotion();
	}

	void Jump(bool executeIfBuffered=false) {
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
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(jumpSpeed, rb2d.velocity.y));
        }

		void WallJump() {
            bufferedJump = false;
			// assume player is facing the wall and needs to be flipped away from it
			jumpNoise.PlayFrom(this.gameObject);
			rb2d.velocity = new Vector2((facingRight ? jumpSpeed : -jumpSpeed)*1.3f, Mathf.Max(jumpSpeed, rb2d.velocity.y));
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
			airJumps--;
			rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(jumpSpeed, rb2d.velocity.y));
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
			} else if (!wallData.touchingWall && !groundData.grounded && airJumps > 0) {
				AirJump();
            }
        }

		if (input.ButtonUp(Buttons.JUMP) && rb2d.velocity.y > jumpCutoffVelocity && canShortHop) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, jumpCutoffVelocity);
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

        if (frozeInputs) {
			animator.SetBool("MovingForward", false);
			animator.SetFloat("XInputMagnitude", 0);
			animator.SetFloat("RelativeXInput", 0);
        } else {
			animator.SetBool("MovingForward", movingForwards);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(input.HorizontalInput()));
			animator.SetFloat("RelativeXInput", input.HorizontalInput() * -transform.localScale.x);
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

	void DropThroughPlatforms(List<RaycastHit2D>platforms) {
		foreach (RaycastHit2D hit in platforms) {
			EdgeCollider2D platform = hit.collider.GetComponent<EdgeCollider2D>();
			if (platform == null) continue;
			platform.enabled = false;
            this.WaitAndExecute(() => platform.enabled = true, 0.5f);
		}
	}

	public void OnAttackGraphEnter() {
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
		return Mathf.Abs(rb2d.velocity.x) > runSpeed + 1.5f;
	}

	public void SetFmod(float f) {
		fMod = f;
	}

	public void OnLedgePop() {
        RefreshAirMovement();
    }

	public void RefreshAirMovement() {
		airDashes = 1;
		airJumps = 1;
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
}
