using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Interactable : MonoBehaviour {
	public Sprite sprite;
	public GameObject optionalPromptPoint;

	GameObject prompt;
	AudioResource hoverSound;

	void Start() {
		prompt = Instantiate(Resources.Load<GameObject>("Runtime/PromptBase"));
		prompt.transform.parent = this.transform;
		if (optionalPromptPoint != null) {
			prompt.transform.position = optionalPromptPoint.transform.position;
		} else {
			prompt.transform.position = this.transform.position + Vector3.up;
		}
		prompt.SetActive(false);

		hoverSound = Resources.Load<AudioResource>("Runtime/InteractHoverSound");
	}

	protected virtual Sprite GetSprite() {
		return sprite;
	}

	public virtual void OnEnter() {
		hoverSound.PlayFrom(this.gameObject);
		prompt.GetComponentInChildren<SpriteRenderer>().sprite = GetSprite();
		prompt.SetActive(true);
	}

	public abstract void OnInteract(EntityController player);

	public virtual void OnExit() {
		prompt.SetActive(false);
	}

	public GameObject GetFlipPoint() {
		if (optionalPromptPoint) return optionalPromptPoint;
		return gameObject;
	}
}
