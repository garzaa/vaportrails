using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SubscriptableInt {
	[SerializeField] int value;
	public IntEvent OnChange;

	public void Initialize() {
		Set(value);
	}
	
	public void Set(int i) {
		value = i;
		OnChange.Invoke(value);
	}

	public int Get() {
		return value;
	}
}

[System.Serializable]
public class IntEvent : UnityEvent<int> {}
