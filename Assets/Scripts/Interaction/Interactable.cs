using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Interactable : MonoBehaviour {
	public Sprite sprite;
	public GameObject optionalPromptPoint;

	GameObject prompt;

	void Start() {
		prompt = Instantiate(Resources.Load<GameObject>("Runtime/InteractPromptTemplate"));
		prompt.GetComponentInChildren<SpriteRenderer>().sprite = GetSprite();
		prompt.transform.parent = this.transform;
		prompt.transform.position = this.transform.position + Vector3.up;
		prompt.SetActive(false);
	}

	protected virtual Sprite GetSprite() {
		return sprite;
	}

	public virtual void OnEnter() {
		prompt.SetActive(true);
	}

	public abstract void OnInteract(EntityController player);

	public virtual void OnExit() {
		prompt.SetActive(false);
	}
}
