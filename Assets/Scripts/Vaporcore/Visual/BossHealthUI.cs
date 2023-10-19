using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BossHealthUI : BarUI {
	public HP bossHP;
	public Text bossName;

	void Start() {
		bossHP.current.OnChange.AddListener(SetCurrent);
		bossHP.max.OnChange.AddListener(SetMax);
		bossHP.OnZero.AddListener(Disable);
		bossName.text = bossHP.gameObject.name;

		// then force-update the listeners because of race conditions...yeah
		SetCurrent(bossHP.current.Get());
		SetMax(bossHP.max.Get());
	}

	void Disable() {
		this.gameObject.SetActive(false);
	}
}
