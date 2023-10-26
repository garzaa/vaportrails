using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class TabUI : SavedObject {
	public List<GameObject> screens = null;
	int currentTab = 0;
	PlayerInput input;
	int tabLeft, tabRight;

	public GameObject tabNameTemplate;
	public GameObject tabNameContainer;
	public GameObject screenContainer;
	public AudioResource navSound;
	public bool smallTab = false;

	protected override void LoadFromProperties() {
		currentTab = Get<int>(nameof(currentTab));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties[nameof(currentTab)] = currentTab;
	}

	void Awake() {
		if (smallTab) {
			tabLeft = RewiredConsts.Action.SmallTabLeft;
			tabRight = RewiredConsts.Action.SmallTabRight;
		} else {
			tabLeft = RewiredConsts.Action.TabLeft;
			tabRight = RewiredConsts.Action.TabRight;
		}
		input = PlayerInput.GetPlayerOneInput();
	}

	void OnEnable() {
		ClearTabNames();

		screens = new List<GameObject>();
		foreach (Transform child in screenContainer.transform) {
			screens.Add(child.gameObject);
		}
		for (int i=0; i<screens.Count; i++) {
			AddTabName(screens[i].name, i);
		}
		ShowTab(currentTab);
		Canvas.ForceUpdateCanvases();
	}

	void Update() {
		if (input.ButtonDown(tabLeft)) {
			navSound.PlayFrom(gameObject);
			ShowTab(currentTab-1);
		} else if (input.ButtonDown(tabRight)) {
			navSound.PlayFrom(gameObject);
			ShowTab(currentTab+1);
		}
	}

	public void ShowTab(int n) {
		if (n < 0) currentTab = screens.Count-1;
		else if (n >= screens.Count) currentTab = 0;
		else currentTab = n;


		for (int i=0; i<screens.Count; i++) {
			if (i != currentTab) screens[i].SetActive(false);
			else screens[i].SetActive(true);
		}

		// this way if the first child item (e.g. item pane) is selected, it won't defocus the button
		for (int i=0; i<tabNameContainer.transform.childCount; i++) {
			Animator a = tabNameContainer.transform.GetChild(i).GetComponent<Animator>();
			a.SetBool("SelectedInNav", i == currentTab);
			if (i != currentTab) a.SetTrigger("Normal");
		}
	}

	void AddTabName(string tabName, int n) {
		GameObject g = Instantiate(tabNameTemplate, tabNameContainer.transform);
		g.GetComponentInChildren<Text>().text = tabName;
		g.GetComponentInChildren<Button>().onClick.AddListener(() => ShowTab(n));
	}

	void ClearTabNames() {
		// don't alter the list while you iterate
		foreach (Transform t in tabNameContainer.transform.Cast<Transform>().ToArray()) {
			t.transform.SetParent(null, worldPositionStays: false);
			Destroy(t.gameObject);
		}
	}
}
