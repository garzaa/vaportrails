using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ConversationContainer))]
public class NPC : Interactable {
	DialogueUI dialogueUI;
	
	Sprite newPromptSprite;
	
	void Awake() {
		dialogueUI = GameObject.FindObjectOfType<DialogueUI>();
		newPromptSprite = Resources.Load<Sprite>("Runtime/UnreadNPCPrompt");
		base.sprite = Resources.Load<Sprite>("Runtime/BaseNPCPrompt");
	}

	protected override Sprite GetSprite() {
		if (GetLastConversations().unread) {
			return newPromptSprite;
		}
		return base.GetSprite();
	}

	public override void OnInteract(EntityController player) {
		if (player == null) {
			player = PlayerInput.GetPlayerOneInput().GetComponent<EntityController>();
		}
		// player should enter cutscene
		dialogueUI.AddLines(GetLastConversations().GetNextConversation());
		dialogueUI.OpenFrom(this.gameObject);
	}

	ConversationContainer GetLastConversations() {
		ConversationContainer[] conversationContainers = GetComponentsInChildren<ConversationContainer>();
		// if conversations are stateful, get the last one
		// this will default to the default conversation since it also looks at self
		return conversationContainers[^1];
	}
}
