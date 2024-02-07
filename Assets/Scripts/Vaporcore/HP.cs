using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;

public class HP : MonoBehaviour, IHitListener {
	public SubscriptableInt current;
	public SubscriptableInt max;
	public UnityEvent OnZero;
	
	public bool renderHealthbar = true;
	[ShowIf(nameof(renderHealthbar))]
	public float verticalOffset = 0.8f;

	public bool sendIntToAnimator = false;
	Animator animator;

	void Start() {
		if (renderHealthbar) {
			// don't have it appear from player stats modification
			StartCoroutine(AddHealthbar());
		}

		max.Initialize();
		current.Initialize();

		animator = GetComponent<Animator>();
		if (sendIntToAnimator) animator.SetInteger("HP", current.Get());
	}

	IEnumerator AddHealthbar() {
		yield return new WaitForEndOfFrame();
		BarUI barUI = Instantiate(Resources.Load<GameObject>("Runtime/MiniHealthBar"), this.transform).GetComponent<BarUI>();
		barUI.normalizeSize = true;
		current.OnChange.AddListener(barUI.SetCurrent);
		max.OnChange.AddListener(barUI.SetMax);
		barUI.SetMax(max.Get());
		barUI.GetComponent<RectTransform>().localPosition = Vector2.up * 0.64f;
	}

	void CheckEvents() {
		if (current.Get() <= 0) {
			OnZero.Invoke();
		}
	}

	public void SetCurrent(int i) {
		i = Mathf.Clamp(i, 0, max.Get());
		current.Set(i);
		if (sendIntToAnimator) animator.SetInteger("HP", i);
		CheckEvents();
	}

	public void SetMax(int i) {
		max.Set(i);
	}

	public void AdjustCurrent(int diff) {
		current.Set(Mathf.Clamp(current.Get()+diff, 0, max.Get()));
		if (sendIntToAnimator) animator.SetInteger("HP", current.Get());
		CheckEvents();
	}

	public void AdjustMax(int diff) {
		max.Set(max.Get() + diff);
	}

	public void OnHit(AttackHitbox attack) {
		AdjustCurrent(-attack.data.GetDamage());
	}

	public void FullHeal() {
		SetCurrent(GetMax());
	}

	public int GetCurrent() {
		return current.Get();
	}

	public int GetMax() {
		return max.Get();
	}
}
