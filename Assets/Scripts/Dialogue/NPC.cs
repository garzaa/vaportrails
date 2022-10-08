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
	}

	protected override Sprite GetSprite() {
		if (GetLastConversations().unread) {
			return newPromptSprite;
		}
		return base.GetSprite();
	}

	public override void OnInteract(EntityController player) {
		// player should enter cutscene
		// oh yeah that should be passed through actually
		dialogueUI.AddLines(GetLastConversations().GetNextConversation());
		dialogueUI.Open(player);
	}

	ConversationContainer GetLastConversations() {
		ConversationContainer[] conversationContainers = GetComponentsInChildren<ConversationContainer>();
		// if conversations are stateful, get the last one
		// this will default to the default conversation
		return conversationContainers[conversationContainers.Length-1];
	}
}
