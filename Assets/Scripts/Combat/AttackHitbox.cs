using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour {
	public bool attacksPlayer;
	
	[ShowIf(nameof(attacksPlayer))] 
	[Tooltip("Attack other enemies as well")]
	public bool indiscriminate;
	
	public AttackData data;
	public bool spawnHitmarkerAtCenter;
	public bool singleHitPerActive = true;
	IAttackLandListener[] attackLandListeners;
	Collider2D[] colliders;
	readonly HashSet<Hurtbox> hurtboxesHitThisActive = new();
	// for entities like Lady of the Lake who has multiple hurtboxes
	readonly HashSet<Entity> entitiesHitThisActive = new();
	CameraZoom cameraZoom;
	bool hitboxOutLastFrame = false;

	public UnityEvent OnAttackLand;

	public bool flipHitmarkerToDirection;
	int i;

	virtual protected void Start() {
		// it can be water. whatever
		if (this is not EnvironmentHitbox) gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		attackLandListeners = GetComponentsInParent<IAttackLandListener>();
		colliders = GetComponents<Collider2D>();
		cameraZoom = GameObject.FindObjectOfType<CameraZoom>();
	}

	void Update() {
		bool hitboxOut = false;
		for (i=0; i<colliders.Length; i++) {
			if (colliders[i].enabled) {
				hitboxOut = true;
				break;
			}
		}

		if (!hitboxOutLastFrame && hitboxOut) {
			OnHitboxOut();
		} else if (!hitboxOut && hitboxOutLastFrame) {
			hurtboxesHitThisActive.Clear();
			entitiesHitThisActive.Clear();
		}

		hitboxOutLastFrame = hitboxOut;
	}

	void OnHitboxOut() {
	
	}

	protected virtual bool CanHit(Hurtbox hurtbox) {
		if (hurtbox.gameObject.CompareTag(Tags.Player) && !attacksPlayer) return false;
		if (attacksPlayer && !hurtbox.gameObject.CompareTag(Tags.Player) && !indiscriminate) return false;

		if (hurtboxesHitThisActive.Contains(hurtbox)) {
			return false;
		}

		Entity e = hurtbox.GetComponentInParent<Entity>();
		if (e && entitiesHitThisActive.Contains(e)) return false;
		if (e && e == GetComponentInParent<Entity>()) return false;

		return true;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (!data) return;
		Hurtbox hurtbox = other.GetComponent<Hurtbox>();
		hurtbox?.HitProbe(this);
		if (hurtbox && CanHit(hurtbox) && hurtbox.VulnerableTo(this)) {
			Hit(hurtbox, other);
		}
	}

	protected virtual void Hit(Hurtbox hurtbox, Collider2D other) {
		if (singleHitPerActive) {
			// if it's an entity, then count all hurtboxes as the same
			if (hurtbox.GetComponentInParent<Entity>() != null) {
				foreach (Hurtbox h in hurtbox.transform.root.GetComponentsInChildren<Hurtbox>()) {
					if (singleHitPerActive) {
						hurtboxesHitThisActive.Add(h);
						Entity e = hurtbox.GetComponentInParent<Entity>();
						if (e) entitiesHitThisActive.Add(e);
					}
				}
			} else {
				// otherwise the root could be a container of random things, like targets
				hurtboxesHitThisActive.Add(hurtbox);
			}
		}

		Collider2D currentActiveCollider = colliders[0];
		foreach (Collider2D col in colliders) {
			if (col.enabled) currentActiveCollider = col;
		}

		if (data.hitSound && hurtbox.hitSoundOverride == null) data.hitSound.PlayFrom(gameObject);
		if (data.hitmarker || hurtbox.hitmarkerOverride) {
			GameObject hitmarkerTemplate = hurtbox.hitmarkerOverride == null ? data.hitmarker : hurtbox.hitmarkerOverride;

			if (spawnHitmarkerAtCenter) Instantiate(hitmarkerTemplate, transform.position, Quaternion.identity);
			else {
				GameObject g = Instantiate(
					hitmarkerTemplate,
					currentActiveCollider.ClosestPoint(other.transform.position+(Vector3)other.GetComponent<Collider2D>().offset),
					Quaternion.identity
				);
				if (flipHitmarkerToDirection) {
					Vector3 v = g.transform.localScale;
					v.x = transform.position.x < other.transform.position.x ? 1 : -1;
					g.transform.localScale = v;
				}
			}
		}

		foreach (IAttackLandListener attackLandListener in attackLandListeners) {
			attackLandListener.OnAttackLand(this, hurtbox);
		}
		hurtbox.OnHitConfirm(this);
		OnAttackLand.Invoke();
		if (data.zoomIn) cameraZoom.ZoomFor(2, data.hitstop);
	}
}
