using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ConversationContainer))]
public class NPC : Interactable {
	DialogueUI dialogueUI;
	
	void Awake() {
		dialogueUI = GameObject.FindObjectOfType<DialogueUI>();
	}

	public override void OnInteract(EntityController player) {
		// player should enter cutscene
		// oh yeah that should be passed through actually
		player.EnterCutscene(this);
		dialogueUI.AddLines(GetLastConversations().GetNextConversation());
	}

	ConversationContainer GetLastConversations() {
		ConversationContainer[] conversationContainers = GetComponentsInChildren<ConversationContainer>();
		// if conversations are stateful, get the last one
		// this will default to the default conversation
		return conversationContainers[conversationContainers.Length-1];
	}
}
