using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Entity {

	#pragma warning disable 0649
	[SerializeField] GameObject playerRig;
	#pragma warning restore 0649

	public float runSpeed = 4.5f;
    public float groundAcceleration = 175;
    public float airAcceleration = 100;
    public float jumpCutoffVelocity = 2f;
    public float jumpForce = 8;
    public float airFriction = 0.7f;
    const float slideFrictionMod = 0.05f;
    const float bufferDuration = 0.2f;

	bool frozeInputs;
	bool justJumped;
	bool justWalkedOffCliff;
	bool bufferedJump;
	float inputX;

	void Start() {

	}

	override protected void Update() {
		base.Update();
		Move();
		Jump();
		UpdateAnimator();
		CheckFlip();
	}

	void FixedUpdate() {
		ApplyMovement();
	}

	void Move() {
		inputX = InputManager.HorizontalInput();

		if (frozeInputs) {
			inputX = 0;
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

	void Jump() {
		if (frozeInputs) return;

		void ExecuteJump() {
            animator.SetTrigger("Jump");
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
			//if the jump button is released
			//then decrease the y velocity to the jump cutoff
			rb2d.velocity = new Vector2(rb2d.velocity.x, jumpCutoffVelocity);
		}
	}

	void UpdateAnimator() {
        animator.SetBool("Grounded", groundData.grounded);
        animator.SetFloat("YSpeed", rb2d.velocity.y);
        animator.SetFloat("XSpeedMagnitude", Mathf.Abs(rb2d.velocity.x));
        animator.SetBool("XInput", Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0);

        if (frozeInputs) {
            animator.SetBool("XInput", false);
        }
    }

	void CheckFlip() {
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
