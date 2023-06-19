using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Hurtbox : MonoBehaviour {
	public AudioResource hitSoundOverride;
	public UnityEvent hitEvent;
	public bool useParentTargetingPosition;
	public bool invisibleToTargeters = false;
	IHitListener[] hitListeners;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		hitListeners = GetComponentsInParent<IHitListener>();
	}

	public bool VulnerableTo(AttackHitbox attack) {
		foreach (IHitListener h in hitListeners) {
			if (!h.CanBeHit(attack)) {
				return false;
			}
		}
		return true;
	}

	public void HitProbe(AttackHitbox attack) {
		foreach (IHitListener hitListener in hitListeners) {
			hitListener.OnHitCheck(attack);
		}
	}

	public void OnHitConfirm(AttackHitbox attack) {
		if (hitSoundOverride != null) {
			hitSoundOverride.PlayFrom(gameObject);
		}
		hitEvent.Invoke();
		
		foreach (IHitListener hitListener in hitListeners) {
			hitListener.OnHit(attack);
		}
	}

	public GameObject GetTargetPosition() {
		if (useParentTargetingPosition) return gameObject;
		else return transform.parent.gameObject;
	}
}
