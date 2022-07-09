using UnityEngine;

public class AttackHitbox : MonoBehaviour {
	public AttackData attackData;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
	}

	// on hit, tell the hurtbox "I would like to hit you"
	// if it accepts, then run hit logic
	// and then propagate it back to the upper entity
}
