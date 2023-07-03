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
	HashSet<Hurtbox> hurtboxesHitThisActive = new HashSet<Hurtbox>();
	// for entities like Lady of the Lake who has multiple hurtboxes
	HashSet<Entity> entitiesHitThisActive = new HashSet<Entity>();
	CameraZoom cameraZoom;
	bool hitboxOutLastFrame = false;

	public UnityEvent OnAttackLand;

	virtual protected void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		attackLandListeners = GetComponentsInParent<IAttackLandListener>();
		colliders = GetComponents<Collider2D>();
		cameraZoom = GameObject.FindObjectOfType<CameraZoom>();
	}

	void Update() {
		bool hitboxOut = false;
		foreach (Collider2D collider in colliders) {
			if (collider.enabled) {
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
		if (data.hitmarker) {
			if (spawnHitmarkerAtCenter) Instantiate(data.hitmarker, transform.position, Quaternion.identity);
			else {
				GameObject g = Instantiate(
					data.hitmarker,
					currentActiveCollider.ClosestPoint(other.transform.position+(Vector3)other.GetComponent<Collider2D>().offset),
					Quaternion.identity
				);
			}
		}

		foreach (IAttackLandListener attackLandListener in attackLandListeners) {
			attackLandListener.OnAttackLand(data, hurtbox);
		}
		OnAttackLand.Invoke();
		hurtbox.OnHitConfirm(this);
		if (data.zoomIn) cameraZoom.ZoomFor(2, data.hitstop);
	}
}
