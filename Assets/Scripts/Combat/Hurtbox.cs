using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Hurtbox : MonoBehaviour {
	public AudioResource hitSoundOverride;
	public UnityEvent hitEvent;
	public bool useParentTargetingPosition;
	IHitListener[] hitListeners;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		hitListeners = GetComponentsInParent<IHitListener>();
	}

	public void OnAttackLand(AttackHitbox attack) {
		foreach (IHitListener hitListener in hitListeners) {
			hitListener.OnHit(attack);
		}
		hitEvent.Invoke();
	}

	public GameObject GetTargetPosition() {
		if (useParentTargetingPosition) return gameObject;
		else return transform.parent.gameObject;
	}
}
