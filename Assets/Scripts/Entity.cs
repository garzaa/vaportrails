using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Entity : MonoBehaviour {
	#pragma warning disable 0649
	[SerializeField] AudioResource defaultFootfall;
	#pragma warning restore 0649

	protected Animator animator;
	protected Rigidbody2D rb2d;
	protected int groundMask;
	protected Collider2D collider2d;
	protected bool facingRight;
	protected GroundData groundData;
	protected Collider2D groundColliderLastFrame;
	
	GroundCheck groundCheck;
	AudioResource currentFootfall;

	static GameObject jumpDust;

	protected virtual void Awake() {
		animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        groundMask = 1 << LayerMask.NameToLayer(Layers.Ground);
        collider2d = GetComponent<Collider2D>();
        groundCheck = GetComponent<GroundCheck>();
        groundData = groundCheck.groundData;
		if (!jumpDust) jumpDust = Resources.Load<GameObject>("Runtime/JumpDust");
	}

	public void JumpDust() {
		Vector2 pos = new Vector2(
			transform.position.x,
			collider2d.bounds.min.y
		);
		Instantiate(jumpDust, pos, Quaternion.identity, null);
	}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround) {
			FootfallSound();
		}
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

	public void Flip() {
        facingRight = !facingRight;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
    }

	protected void WaitAndExecute(Action action, float timeout) {
        StartCoroutine(_WaitAndExecute(action, timeout));
    }

    IEnumerator _WaitAndExecute(Action action, float timeout) {
        yield return new WaitForSeconds(timeout);
        action();
    }
}
