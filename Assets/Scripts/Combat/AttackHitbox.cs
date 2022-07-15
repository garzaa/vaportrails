using UnityEngine;

public class AttackHitbox : MonoBehaviour {
	public bool attacksPlayer;
	public AttackData attackData;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
	}

	protected virtual bool CanHit(Entity entity) {
		if (entity.gameObject.CompareTag(Tags.Player) && !attacksPlayer) return false;
		return true;
	}

	// on hit, tell the hurtbox "I would like to hit you"
	// if it accepts, then run hit logic
	// and then propagate it back to the upper entity
}
