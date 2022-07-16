using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EntityShader : MonoBehaviour, IHitListener {
	List<Renderer> renderers = new List<Renderer>();
	List<MaterialPropertyBlock> propertyBlocks = new List<MaterialPropertyBlock>();

	Material entityMaterial;

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
		ExecuteChange(block => block.SetFloat("whiteFlashTime", Time.time));
	}

	public void FlashCyan() {
		ExecuteChange(block => block.SetFloat("cyanFlashTime", Time.time));
	}

	void ExecuteChange(Action<MaterialPropertyBlock> action) {
		for (int i=0; i<renderers.Count; i++) {
			renderers[i].GetPropertyBlock(propertyBlocks[i]);
			action(propertyBlocks[i]);
			renderers[i].SetPropertyBlock(propertyBlocks[i]);
		}
	}

	public void OnHit(AttackHitbox attack) {
		FlashWhite();
	}
}
