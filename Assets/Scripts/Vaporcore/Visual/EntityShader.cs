using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EntityShader : MonoBehaviour {
	List<Renderer> renderers = new List<Renderer>();
	List<MaterialPropertyBlock> propertyBlocks = new List<MaterialPropertyBlock>();

	Material entityMaterial;

	Coroutine flinchRoutine;

	void Awake() {
		entityMaterial = Resources.Load<Material>("Runtime/EntityMaterial");
		renderers = new List<Renderer>(GetComponentsInChildren<SpriteRenderer>());

		for (int i=0; i<renderers.Count; i++) {
			propertyBlocks.Add(new MaterialPropertyBlock());
			renderers[i].material = entityMaterial;
			renderers[i].GetPropertyBlock(propertyBlocks[i]);
		}
	}

	public void FlashWhite() {
		ExecuteChange(block => block.SetFloat("whiteFlashTime", Time.unscaledTime));
	}

	public void FlashCyan() {
		ExecuteChange(block => block.SetFloat("cyanFlashTime", Time.unscaledTime));
	}

	public void StartFlashingWhite() {
		ExecuteChange(block => {
			block.SetFloat("whiteFlashWeight", 1);
		});
	}

	public void StopFlashingWhite() {
		ExecuteChange(block => {
			block.SetFloat("whiteFlashWeight", 0);
		});
	}

	void ExecuteChange(Action<MaterialPropertyBlock> action) {
		for (int i=0; i<renderers.Count; i++) {
			renderers[i].GetPropertyBlock(propertyBlocks[i]);
			action(propertyBlocks[i]);
			renderers[i].SetPropertyBlock(propertyBlocks[i]);
		}
	}

	public void Flinch(Vector2 direction, float duration) {
		if (direction.sqrMagnitude == 0) {
			direction = Vector2.right;
		}
		if (flinchRoutine != null) StopCoroutine(flinchRoutine);
		ExecuteChange(block => {
			block.SetVector("flinchDirection", direction.normalized);
			block.SetFloat("flinchWeight", 1f);
		});
		flinchRoutine = StartCoroutine(StopFlinch(duration));
	}

	public void FlinchOnce(Vector2 direction) {
		Flinch(direction, 0.1f);
	}

	IEnumerator StopFlinch(float duration) {
		yield return new WaitForSeconds(duration);
		ExecuteChange(block => {
			block.SetFloat("flinchWeight", 0f);
		});
	}
	
	void OnDestroy() {
		foreach (Renderer renderer in renderers) {
			Destroy(renderer.material);	
		}
	}
}
