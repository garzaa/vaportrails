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

	BarUI barUI;

	void Start() {
		if (renderHealthbar) {
			barUI = Instantiate(Resources.Load<GameObject>("Runtime/MiniHealthBar"), this.transform).GetComponent<BarUI>();
			current.OnChange.AddListener(barUI.SetCurrent);
			max.OnChange.AddListener(barUI.SetMax);
			barUI.GetComponent<RectTransform>().localPosition = Vector2.up * 0.64f;
			barUI.HideImmediate();
		}

		max.Initialize();
		current.Initialize();
	}

	void CheckEvents() {
		if (current.Get() <= 0) {
			OnZero.Invoke();
		}
	}

	public void SetCurrent(int i, bool quiet=false) {
		current.Set(i);
		CheckEvents();
		if (quiet) barUI.HideImmediate(); 
	}

	public void SetMax(int i) {
		max.Set(i);
	}

	public void AdjustCurrent(int diff) {
		current.Set(Mathf.Min(current.Get() + diff, max.Get()));
		CheckEvents();
	}

	public void AdjustMax(int diff) {
		max.Set(max.Get() + diff);
	}

	public void OnHit(AttackHitbox attack) {
		AdjustCurrent(-attack.data.GetDamage());
	}

	public void FullHeal() {
		current.Set(max.Get());
	}

	public int GetCurrent() {
		return current.Get();
	}

	public int GetMax() {
		return max.Get();
	}
}
