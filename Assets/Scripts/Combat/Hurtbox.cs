using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Hurtbox : MonoBehaviour {
	public AudioResource hitSoundOverride;
	public UnityEvent hitEvent;
	public bool useParentTargetingPosition;
	public bool invisibleToTargeters = false;
	public bool takesEnvironmentDamage = true;
	public GameObject hitmarkerOverride;
	IHitListener[] hitListeners;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		hitListeners = GetComponentsInParent<IHitListener>();
	}

	public virtual bool VulnerableTo(AttackHitbox attack) {
		if (attack is EnvironmentHitbox && !takesEnvironmentDamage) return false;
		if (hitListeners != null) {
			foreach (IHitListener h in hitListeners) {
				if (!h.CanBeHit(attack)) {
					return false;
				}
			}
		}
		return true;
	}

	public void HitProbe(AttackHitbox attack) {
		if (hitListeners != null) {
			foreach (IHitListener hitListener in hitListeners) {
				hitListener.OnHitCheck(attack);
			}
		}
	}

	public void OnHitConfirm(AttackHitbox attack) {
		hitSoundOverride?.PlayFrom(gameObject);
		hitEvent.Invoke();
		
		if (hitListeners != null) {
			foreach (IHitListener hitListener in hitListeners) {
				hitListener.OnHit(attack);
			}
		}
	}

	public GameObject GetTargetPosition() {
		if (useParentTargetingPosition) return gameObject;
		else return transform.parent.gameObject;
	}

	public virtual Vector2? KnockbackOverride(AttackHitbox attack) {
		return null;
	}
}
