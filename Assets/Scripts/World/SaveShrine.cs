using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SaveShrine : SavedObject, IPlayerEnterListener {
	bool unlocked = false;
	Animator animator;
	public GameObject headTracker;
	
	public GameObject hackInteractable;

	Transform playerHead;
	Entity player;

	public UnityEvent OnUnlock;

	protected override void Initialize() {
		animator = GetComponent<Animator>();
		player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
		playerHead = player.transform.Find("PlayerRig/Hips/Chest/Head");
		headTracker.SetActive(false);
	}

	protected override void LoadFromProperties() {
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
			player.FlipTo(this.gameObject);
			player.EnterCutscene(this.gameObject);
			
		}
		hackInteractable.SetActive(false);
	}

	public void OnPlayerEnter(Collider2D player) {
		if (unlocked) {
			headTracker.transform.parent = playerHead;
			headTracker.transform.localPosition = Vector3.zero; 
			headTracker.transform.localScale = Vector3.one;
			headTracker.SetActive(true);
			SaveManager.Save();
			player.GetComponent<HP>().FullHeal();
			animator.SetTrigger("PlaySaveEffect");
		}
	}

	public void OnPlayerExit() {
		headTracker.gameObject.SetActive(false);
		headTracker.transform.parent = this.transform;
	}

	public void FinishHackAnimation() {
		OnUnlock.Invoke();
		OnPlayerEnter(PlayerInput.GetPlayerOneInput().GetComponent<Collider2D>());
		player.ExitCutscene(this.gameObject);
	}

	public void SetPlayerKneeling() {
		PlayerInput.GetPlayerOneInput().GetComponent<Animator>().Play("KneelHandUp");
	}

	public void ResetToIdle() {
		PlayerInput.GetPlayerOneInput().GetComponent<Animator>().SetTrigger("ResetToIdle");
	}
}
