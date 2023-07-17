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

// TODO: make it persist the same tab between scenes
// saved object? 
	void Start() {
		ClearTabNames();

		screens = new List<GameObject>();
		foreach (Transform child in screenContainer.transform) {
			Debug.Log("Adding screen " + child.name);
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
		ShowTab(0);
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

	void ShowTab(int n) {
		if (n < 0) currentTab = screens.Count-1;
		else if (n >= screens.Count) currentTab = 0;

		Debug.Log("showing tab "+currentTab);

		for (int i=0; i<screens.Count; i++) {
			if (i != currentTab) screens[i].SetActive(false);
			else screens[i].SetActive(true);
		}
	}

	void AddTabName(string tabName, int n) {
		Debug.Log("adding tab "+tabName);
		GameObject g = Instantiate(tabNameTemplate, tabNameContainer.transform);
		g.GetComponentInChildren<Text>().text = tabName;
		g.GetComponentInChildren<Button>().onClick.AddListener(() => ShowTab(n));
	}

	void ClearTabNames() {
		foreach (Transform t in tabNameContainer.transform) {
			Debug.Log("removeing tab "+t.gameObject.name);
			t.transform.SetParent(null, worldPositionStays: false);
			Destroy(t.gameObject);
		}
	}
}
