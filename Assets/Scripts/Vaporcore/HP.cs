using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class HP : MonoBehaviour, IHitListener {
	[SerializeField] SubscriptableInt current;
	[SerializeField] SubscriptableInt max;
	public UnityEvent OnZero;
	
	public bool renderHealthbar = true;
	[ShowIf(nameof(renderHealthbar))]
	public float verticalOffset = 0.8f;

	void Start() {
		if (renderHealthbar) {
			BarUI barUI = Instantiate(Resources.Load<GameObject>("Runtime/MiniHealthBar"), this.transform).GetComponent<BarUI>();
			current.OnChange.AddListener(barUI.SetCurrent);
			max.OnChange.AddListener(barUI.SetMax);
			barUI.GetComponent<RectTransform>().localPosition = Vector2.up * 0.64f;
		}

		max.Initialize();
		current.Initialize();
	}

	void CheckEvents() {
		if (current.Get() <= 0) {
			OnZero.Invoke();
		}
	}

	public void SetCurrent(int i) {
		current.Set(i);
		CheckEvents();
	}

	public void SetMax(int i) {
		max.Set(i);
	}

	public void AdjustCurrent(int diff) {
		current.Set(current.Get() + diff);
		CheckEvents();
	}

	public void AdjustMax(int diff) {
		max.Set(max.Get() + diff);
	}

	public void OnHit(AttackHitbox attack) {
		AdjustCurrent(-attack.data.damage);
	}

	public void FullHeal() {
		current.Set(max.Get());
	}
}
