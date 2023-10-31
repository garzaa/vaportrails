using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Globalization;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Achievements : SavedObject {

#if UNITY_EDITOR
	[MenuItem("GameObject/Vapor Trails/Create Achievement")]
	public static void CreateAchievement() {
		Achievement a = ScriptableObject.CreateInstance<Achievement>();
		AssetDatabase.CreateAsset(a, "Assets/Resources/Runtime/Achievements/NewAchievement.asset");
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = a;
	}
#endif

	HashSet<string> achievements = new();
	Achievement[] loadedAchievements = null;

	public Transform unlockedContainer;
	public Transform lockedContainer;
	public GameObject achievementPrefab;
	public Sprite unlockedFrame;
	public Sprite rareFrame;

	public Animator animator;
	public Text unlockTitle;
	public Image unlockIcon;

	public Text unlockedText;
	int unlockedCount;
	int totalCount;

    protected override void LoadFromProperties() {
        achievements = GetHashSet<string>(nameof(achievements));
    }

    protected override void SaveToProperties(ref Dictionary<string, object> properties) {
        properties[nameof(achievements)] = achievements;
    }

	public bool Has(Achievement a) {
		return achievements.Contains(a.GetName());
	}

public void Get(Achievement a) {
	if (!Has(a)) {
			achievements.Add(a.GetName());
			NotifyUnlock(a);
		}
	}

	public void NotifyUnlock(Achievement a) {
		unlockTitle.text = a.GetName();
		unlockIcon.sprite = a.Icon;
		animator.SetTrigger("Unlock");
	}

	public void ListAchievements() {
		UtilityMethods.ClearUIList(lockedContainer);
		UtilityMethods.ClearUIList(unlockedContainer);
		unlockedCount = 0;
		totalCount = 0;

		loadedAchievements ??= Resources.LoadAll<Achievement>("Runtime/Achievements");
		foreach (Achievement a in loadedAchievements) {
			AddUIPrefab(a);
		}

		unlockedText.text = $"Unlocked: {unlockedCount}/{totalCount}";
	}

	void AddUIPrefab(Achievement a) {
		GameObject g = Instantiate(achievementPrefab, lockedContainer);
		Text[] textObjects = g.GetComponentsInChildren<Text>();
		Image[] images = g.GetComponentsInChildren<Image>();
		textObjects[0].text = a.GetName();
		textObjects[1].text = a.Description;
		images[2].sprite = a.Icon;
		totalCount++;
		if (Has(a)) {
			unlockedCount++;
			g.transform.SetParent(unlockedContainer, worldPositionStays: false);
			textObjects[0].color = new Color32(199, 207, 221, 255);
			if (a.Rare) {
				images[1].sprite = rareFrame;
			} else {
				images[1].sprite = unlockedFrame;
			}
			// the shader rounds to 0 so it doesn't get culled out automatically if its color.a is set to 0
			images[2].color = new Color32(255, 255, 255, 100);
		} else {
			if (a.Secret) {
				textObjects[1].text = "???";
			}
			images[2].color = new Color32(33, 17, 88, 255);
		}
	}

}	
