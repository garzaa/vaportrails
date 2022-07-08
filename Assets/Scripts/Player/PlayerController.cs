using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Entity {

	#pragma warning disable 0649
	[SerializeField] GameObject playerRig;
	[SerializeField] AudioResource jumpNoise;
	[SerializeField] AudioResource landNoise;
	#pragma warning restore 0649

	const float runSpeed = 4.5f;
    const float groundAcceleration = 175;
    const float airAcceleration = 80;
    const float jumpCutoffVelocity = 2f;
    const float jumpForce = 8;
    const float airFriction = 0.3f;
    const float slideFrictionMod = 0.05f;
    const float bufferDuration = 0.1f;
	const float maxFallSpeed = -10f;
	const float maxWallSlideSpeed = -4f;
	const float dashCooldown = 0.7f;
	float dashForce = 8;
	float airControlMod = 1;
	float fMod = 1;
	float fModRecoveryRate = 2;

	bool frozeInputs;
	bool inputBackwards;
	bool movingBackwards;
	bool justWalkedOffCliff;
	bool justLeftWall;
	bool bufferedJump;
	bool movingForward;
	bool speeding;
	bool canDash = true;
	float inputX;
	float landingRecovery = 1;

	ToonMotion toonMotion;
	WallCheckData wallData;
	GameObject wallJumpDust;

	override protected void Awake() {
		base.Awake();
		toonMotion = GetComponentInChildren<ToonMotion>();
		wallData = GetComponent<WallCheck>().wallData;
		wallJumpDust = Resources.Load<GameObject>("Runtime/WallJumpDust");
	}

	override protected void Update() {
		base.Update();
		CheckFlip();
		Move();
		Dash();
		Jump();
		UpdateAnimator();
	}

	void FixedUpdate() {
		ApplyMovement();
	}

	void Move() {
		inputX = InputManager.HorizontalInput();
		inputBackwards = InputManager.HasHorizontalInput()
				&& Mathf.Abs(rb2d.velocity.x) > 0
				&& InputManager.HorizontalInput()*rb2d.velocity.x < 0;
		movingBackwards = Mathf.Abs(rb2d.velocity.x) > 0.01 && rb2d.velocity.x * -transform.localScale.x < 0;
		movingForward = InputManager.HasHorizontalInput() && ((facingRight && rb2d.velocity.x > 0) || (!facingRight && rb2d.velocity.x < 0));
		airControlMod = Mathf.MoveTowards(airControlMod, 1, 1f * Time.deltaTime);

		if (frozeInputs) {
			inputX = 0;
			inputBackwards = false;
		}

		if (groundData.leftGround) {
            if (rb2d.velocity.y <= 0) {
                justWalkedOffCliff = true;
                WaitAndExecute(() => justWalkedOffCliff = false, bufferDuration);
            }

			// the player can initiate walltouch on the ground
			// and ground movement can override the wallflip
			if (wallData.touchingWall) {
				FlipToWall();
			}
        }

		if (groundData.hitGround) {
			landNoise.PlayFrom(this.gameObject);
		}

		if (wallData.hitWall) {
			OnWallHit();
		}
	}

	void OnWallHit() {
		// play the hit sound
		landNoise.PlayFrom(this.gameObject);
		FlipToWall();
		// add the dust effect for hitting the wall
		GameObject g = Instantiate(landDust);
		float x = facingRight ? collider2d.bounds.min.x : collider2d.bounds.max.x;
		g.transform.position = new Vector2(x, transform.position.y);
		g.transform.eulerAngles = new Vector3(0, 0, facingRight ? -90 : 90);
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

        if (inputX!=0) {
			if (!speeding || inputBackwards) {
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

		if (wallData.leftWall) {
			justLeftWall = true;
			WaitAndExecute(()=>justLeftWall=false, bufferDuration*2);
		}

		Mathf.MoveTowards(fMod, 1, (1f/fModRecoveryRate) * Time.fixedDeltaTime);
	}

	void Dash() {
		void EndDashCooldown() {
			if (canDash) return;
			entityShader.FlashCyan();
			canDash = true;
		}

		if (frozeInputs) return;
		if (InputManager.ButtonDown(Buttons.SPECIAL) && canDash) {
			rb2d.AddForce(Vector2.right * InputManager.HorizontalInput() * dashForce, ForceMode2D.Impulse);
			animator.SetTrigger("Dash");
			entityShader.FlashWhite();
			canDash = false;
			WaitAndExecute(EndDashCooldown, dashCooldown);
		}
	}

	void Jump() {
		if (frozeInputs) return;

		void GroundJump() {
			jumpNoise.PlayFrom(this.gameObject);
			if (!wallData.touchingWall) {
				if (inputBackwards || movingBackwards) {
					animator.SetTrigger("Backflip");
				} else {
					animator.SetTrigger("Jump");
				}
			}
			JumpDust();
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(0, rb2d.velocity.y));
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

		void WallJump() {
			// assume player is facing the wall and needs to be flipped away from it
			jumpNoise.PlayFrom(this.gameObject);
			rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(0, rb2d.velocity.y));
            rb2d.AddForce(
				new Vector2((facingRight ? jumpForce : -jumpForce)*1.3f, jumpForce),
				ForceMode2D.Impulse
			);
			airControlMod = 0;
			GameObject w = Instantiate(wallJumpDust);
			w.transform.position = new Vector2(facingRight ? collider2d.bounds.min.x : collider2d.bounds.max.x, transform.position.y);
			w.transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
			WaitAndExecute(() => animator.SetTrigger("WallJump"), 0.1f);
		}

		if (groundData.hitGround && bufferedJump) {
            bufferedJump = false;
            GroundJump();
            return;
        } else if (wallData.hitWall && bufferedJump) {
			bufferedJump = false;
			WallJump();
			return;
		}

		if (InputManager.ButtonDown(Buttons.JUMP)) {
            if (groundData.grounded || justWalkedOffCliff) {
                if (groundData.platforms.Count > 0 && Input.GetAxisRaw("Vertical") < -0.8f) {
					DropThroughPlatforms(groundData.platforms);
					return;
				}
                GroundJump();
            } else if (wallData.touchingWall || justLeftWall) {
				WallJump();
			} else {
                bufferedJump = true;
                WaitAndExecute(() => bufferedJump = false, bufferDuration);
            }
        }

		if (InputManager.ButtonUp(Buttons.JUMP) && rb2d.velocity.y > jumpCutoffVelocity) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, jumpCutoffVelocity);
		}
	}

	void UpdateAnimator() {
        animator.SetBool("Grounded", groundData.grounded);
        animator.SetFloat("YSpeed", rb2d.velocity.y);
        animator.SetFloat("XSpeedMagnitude", Mathf.Abs(rb2d.velocity.x));
		animator.SetBool("MovingBackward", inputBackwards);
		animator.SetBool("Wallsliding", wallData.touchingWall);

        if (frozeInputs) {
			animator.SetBool("MovingForward", false);
			animator.SetFloat("XInputMagnitude", 0);
			animator.SetFloat("RelativeXInput", 0);
        } else {
			animator.SetBool("MovingForward", movingForward);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(InputManager.HorizontalInput()));
			animator.SetFloat("RelativeXInput", InputManager.HorizontalInput() * -transform.localScale.x);
		}

		if (groundData.hitGround) {
			StartCoroutine(UpdateToonMotion());
			landingRecovery = -1;
		}

		if (groundData.grounded) {
			animator.SetBool("FallInterrupt", true);
		}

		landingRecovery = Mathf.MoveTowards(landingRecovery, 0, 4f * Time.deltaTime);
		animator.SetFloat("LandingRecovery", landingRecovery);
    }
	
	IEnumerator UpdateToonMotion() {
		yield return new WaitForEndOfFrame();
		toonMotion.ForceUpdate();
	}

	void CheckFlip() {
		if (inputBackwards || !groundData.grounded) return;
		
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
            WaitAndExecute(() => platform.enabled = true, 0.5f);
		}
	}
}
