using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConversationContainer : MonoBehaviour {
	#pragma warning disable 0649
	[SerializeField] List<Conversation> conversations = new List<Conversation>();
	#pragma warning restore 0649

	int currentConversation = 0;

	public List<DialogueLine> GetNextConversation() {
		if (currentConversation == conversations.Count-1) {
			return conversations[currentConversation].lines;
		}
		return conversations[currentConversation++].lines;
	}

	public bool HasNext() {
		return currentConversation < conversations.Count-1;
	}

	[System.Serializable]
	public class Conversation {
		public List<DialogueLine> lines;
	}
}
