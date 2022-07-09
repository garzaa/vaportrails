using UnityEngine;

public class Hurtbox : MonoBehaviour {
	public AudioResource hitSoundOverride;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
	}

	// verification method for an attack wanting to hit

	// and then logic for propagating the hit back to the parent entity
}
