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
    const float airFriction = 0.5f;
    const float slideFrictionMod = 0.05f;
    const float bufferDuration = 0.2f;
	public float dashForce = 8;

	bool frozeInputs;
	bool justJumped;
	bool inputBackwards;
	bool movingBackwards;
	bool justWalkedOffCliff;
	bool bufferedJump;
	float inputX;
	float landingRecovery = 1;

	ToonMotion toonMotion;

	override protected void Awake() {
		base.Awake();
		toonMotion = GetComponentInChildren<ToonMotion>();
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

		if (frozeInputs) {
			inputX = 0;
			inputBackwards = false;
		}

		if (groundData.leftGround) {
            // due to physics updating, run this here and make it velocity based
            if (rb2d.velocity.y > 0) {
                justJumped = true;
                WaitAndExecute(() => justJumped = false, bufferDuration);
            } else {
                justWalkedOffCliff = true;
                WaitAndExecute(() => justWalkedOffCliff = false, bufferDuration);
            }
        }

		if (groundData.hitGround) {
			landNoise.PlayFrom(this.gameObject);
		}
	}

	void ApplyMovement() {
		void SlowOnFriction() {
            float f = groundData.grounded ? groundData.groundCollider.friction : airFriction;
            rb2d.velocity = new Vector2(rb2d.velocity.x * (1 - (f*f)), rb2d.velocity.y);
        }

        if (inputX!=0) {
            if (groundData.grounded) {
                // if ground is a gnashable platform that's been gnashed/destroyed
                float f = groundData.groundCollider != null ? groundData.groundCollider.friction : airFriction;
                rb2d.AddForce(Vector2.right * rb2d.mass * groundAcceleration * inputX * f*f);
            } else {
                rb2d.AddForce(Vector2.right * rb2d.mass * airAcceleration * inputX);
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

        // reduce speed if speeding
        if (Mathf.Abs(rb2d.velocity.x) > runSpeed) {
            SlowOnFriction();
        }
	}

	void Dash() {
		if (frozeInputs) return;
		if (InputManager.ButtonDown(Buttons.SPECIAL)) {
			rb2d.AddForce(Vector2.right * InputManager.HorizontalInput() * dashForce, ForceMode2D.Impulse);
		}
	}

	void Jump() {
		if (frozeInputs) return;

		void ExecuteJump() {
			jumpNoise.PlayFrom(this.gameObject);
			// backflip can flip on the same frame...how to deal with this
            if (inputBackwards || movingBackwards) {
				animator.SetTrigger("Backflip");
			} else {
				animator.SetTrigger("Jump");
			}
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Max(0, rb2d.velocity.y));
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

		if (groundData.hitGround && bufferedJump) {
            bufferedJump = false;
            ExecuteJump();
            return;
        }

		if (InputManager.ButtonDown(Buttons.JUMP)) {
            if (groundData.grounded || justWalkedOffCliff) {
                if (groundData.platforms.Count > 0 && Input.GetAxisRaw("Vertical") < -0.8f) {
					DropThroughPlatforms(groundData.platforms);
					return;
				}
                ExecuteJump();
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

        if (frozeInputs) {
			animator.SetBool("MovingForward", false);
			animator.SetFloat("XInputMagnitude", 0);
			animator.SetFloat("RelativeXInput", 0);
        } else {
			bool movingForward = InputManager.HasHorizontalInput() && ((facingRight && rb2d.velocity.x > 0) || (!facingRight && rb2d.velocity.x < 0));
			animator.SetBool("MovingForward", movingForward);
			animator.SetFloat("XInputMagnitude", Mathf.Abs(InputManager.HorizontalInput()));
			animator.SetFloat("RelativeXInput", InputManager.HorizontalInput() * -transform.localScale.x);
		}

		if (groundData.hitGround) {
			toonMotion.ForceUpdate();
			landingRecovery = -1;
		}
		landingRecovery = Mathf.MoveTowards(landingRecovery, 0, 4f * Time.deltaTime);
		animator.SetFloat("LandingRecovery", landingRecovery);
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
