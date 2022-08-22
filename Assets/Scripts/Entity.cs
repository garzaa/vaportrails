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
	#pragma warning restore 0649

	protected Animator animator;
	protected Rigidbody2D rb2d;
	protected int groundMask;
	protected Collider2D collider2d;
	protected bool facingRight;
	protected GroundData groundData;
	protected WallCheckData wallData;
	protected Collider2D groundColliderLastFrame;
	protected EntityShader entityShader;
	protected bool stunned = false;
	
	GroundCheck groundCheck;
	AudioResource currentFootfall;
	PhysicsMaterial2D defaultMaterial;

	static GameObject jumpDust;
	protected static GameObject landDust;
	protected static PhysicsMaterial2D stunMaterial;
	static GameObject footfallDust;
	ParticleSystem stunSmoke;
	
	bool canGroundHitEffect = true;
	public bool staggerable = true;

	protected virtual void Awake() {
		animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
		entityShader = GetComponent<EntityShader>();
        groundMask = 1 << LayerMask.NameToLayer(Layers.Ground);
        collider2d = GetComponent<Collider2D>();
        groundCheck = GetComponent<GroundCheck>();
        groundData = groundCheck.groundData;
		wallData = GetComponent<WallCheck>().wallData;
		if (!jumpDust) jumpDust = Resources.Load<GameObject>("Runtime/JumpDust");
		if (!landDust) landDust = Resources.Load<GameObject>("Runtime/LandDust");
		if (!footfallDust) footfallDust = Resources.Load<GameObject>("Runtime/FootfallDust");
		if (!stunMaterial) stunMaterial = Resources.Load<PhysicsMaterial2D>("Runtime/BounceEntity");
		defaultMaterial = rb2d.sharedMaterial;
		stunSmoke = Instantiate(Resources.Load<GameObject>("Runtime/StunSmoke"), this.transform).GetComponent<ParticleSystem>();
		stunSmoke.transform.localPosition = Vector3.zero;
		stunSmoke.Stop();
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
			// TODO: alter knockback vector based on hurtbox point distance from line of attack center + knockback vector
			// https://answers.unity.com/questions/263308/projection-of-a-point-on-a-line.html
			Vector2 v = attack.data.knockback;
			float attackX = attack.transform.position.x;
			v.x *= attackX > transform.position.x ? -1 : 1;
			// heavier people get knocked back less
			rb2d.velocity = v * (1f/rb2d.mass);
			// flip to attack
			if (facingRight && attackX<transform.position.x) {
				Flip();
			} else if (!facingRight && attackX>transform.position.x) {
				Flip();
			}
			StunFor(attack.data.stunLength);
		}
	}

	void StunFor(float seconds) {
		CancelStun();
		stunned = true;
		rb2d.sharedMaterial = stunMaterial;
		stunSmoke.Play();
		Invoke(nameof(UnStun), seconds);
	}

	protected void CancelStun() {
		if (IsInvoking(nameof(UnStun))) {
			CancelInvoke(nameof(UnStun));
		}
		rb2d.sharedMaterial = defaultMaterial;
		stunned = false;
		stunSmoke.Stop();
	}

	void UnStun() {
		rb2d.sharedMaterial = defaultMaterial;
		stunned = false;
		stunSmoke.Stop();
	}

	protected virtual void Update() {
		UpdateFootfallSound();
		if (groundData.hitGround && canGroundHitEffect) {
			FootfallSound();
			LandDust();
			canGroundHitEffect = false;
			WaitAndExecute(() => canGroundHitEffect=true, 0.1f);
		}
		if (wallData.hitWall) {
			landNoise.PlayFrom(this.gameObject);
			GameObject g = Instantiate(landDust);
			bool wallRight = wallData.direction > 0;
			float x = wallRight ? collider2d.bounds.max.x : collider2d.bounds.min.x;
			g.transform.position = new Vector2(x, transform.position.y);
			g.transform.eulerAngles = new Vector3(0, 0, wallRight ? 90 : -90);
			OnWallHit();
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

	public Vector2Int Forward() {
		return new Vector2Int(
            ForwardScalar(),
            1
        );
	}

	public int ForwardScalar() {
		return facingRight ? 1 : -1;
	}

	public void AddAttackImpulse(Vector2 impulse) {
		rb2d.AddForce(impulse * Forward(), ForceMode2D.Impulse);
	}
}
