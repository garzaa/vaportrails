using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TabUI : MonoBehaviour {
	List<GameObject> screens = null;
	int currentTab = 0;
	PlayerInput input;
	int tabLeft, tabRight;

	public GameObject tabNameTemplate;
	public GameObject tabNameContainer;
	public GameObject screenContainer;
	public AudioResource navSound;
	public bool smallTab = false;

	void Start() {
		ClearTabNames();

		screens = new List<GameObject>();
		foreach (Transform child in screenContainer.transform) {
			screens.Add(child.gameObject);
		}
		input = PlayerInput.GetPlayerOneInput();
		
		if (smallTab) {
			tabLeft = RewiredConsts.Action.SmallTabLeft;
			tabRight = RewiredConsts.Action.SmallTabRight;
		} else {
			tabLeft = RewiredConsts.Action.TabLeft;
			tabRight = RewiredConsts.Action.TabRight;
		}

		for (int i=0; i<screens.Count; i++) {
			AddTabName(screens[i].name, i);
		}
		Canvas.ForceUpdateCanvases();
	}

	void Update() {
		if (!input) Start();

		if (input.ButtonDown(tabLeft)) {
			navSound.PlayFrom(gameObject);
			ShowTab(currentTab-1);
		} else if (input.ButtonDown(tabRight)) {
			navSound.PlayFrom(gameObject);
			ShowTab(currentTab+1);
		}
	}

	void ShowTab(int tabNum) {
		if (currentTab < 0) currentTab = screens.Count-1;
		else if (currentTab >= screens.Count) currentTab = 0;

		currentTab = tabNum;
		for (int i=0; i<screens.Count; i++) {
			if (i != tabNum) screens[i].SetActive(false);
			else screens[i].SetActive(true);
		}
	}

	void AddTabName(string tabName, int n) {
		GameObject g = Instantiate(tabNameTemplate, tabNameContainer.transform);
		g.GetComponentInChildren<Text>().text = tabName;
		g.GetComponentInChildren<Button>().onClick.AddListener(() => ShowTab(n));
	}

	void ClearTabNames() {
		foreach (Transform t in tabNameContainer.transform) {
			t.transform.SetParent(null, worldPositionStays: false);
			Destroy(t.gameObject);
		}
	}
}
