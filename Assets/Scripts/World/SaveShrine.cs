using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

public class SaveShrine : SavedObject, IPlayerEnterListener {
	bool unlocked = false;
	Animator animator;
	public GameObject headTracker;
	
	public GameObject hackInteractable;

	protected override void Initialize() {
		animator = GetComponent<Animator>();
		ConstraintSource s = headTracker.GetComponent<ParentConstraint>().GetSource(0);
		s.sourceTransform = PlayerInput.GetPlayerOneInput().transform.Find("PlayerRig/Hips/Chest/Head");
		headTracker.GetComponent<ParentConstraint>().SetSource(0, s);
		headTracker.GetComponent<ScaleConstraint>().SetSource(0, s);
		headTracker.SetActive(false);
	}

	protected override void LoadFromProperties(bool startingUp) {
		unlocked = Get<bool>(nameof(unlocked));
		animator.SetBool("Unlocked", unlocked);
		if (unlocked) {
			hackInteractable.SetActive(false);
		}
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties[nameof(unlocked)] = unlocked;
	}

	public void InteractEvent() {
		if (!unlocked) {
			unlocked = true;
			animator.SetTrigger("Unlock");
		}
		hackInteractable.SetActive(false);
	}

	public void OnPlayerEnter(Collider2D player) {
		if (unlocked) {
			headTracker.SetActive(true);
			FindObjectOfType<SaveManager>().Save();
			animator.SetTrigger("PlaySaveEffect");
		}
	}

	public void OnPlayerExit() {
		headTracker.gameObject.SetActive(false);
	}

	public void FinishHackAnimation() {
		OnPlayerEnter(null);
	}

	public void SetPlayerKneeling() {
		PlayerInput.GetPlayerOneInput().GetComponent<Animator>().Play("KneelHandUp");
	}

	public void ResetToIdle() {
		PlayerInput.GetPlayerOneInput().GetComponent<Animator>().SetTrigger("ResetToIdle");
	}
}
