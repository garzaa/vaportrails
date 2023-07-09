using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConversationContainer : SavedObject {
	#pragma warning disable 0649
	[SerializeField] List<Conversation> conversations = new List<Conversation>();
	#pragma warning restore 0649

	int currentConversation = 0;

	public bool unread => (currentConversation < conversations.Count);

	protected override void LoadFromProperties(bool startingUp) {
		currentConversation = Get<int>("currentConversation");
	}

	public List<DialogueLine> GetNextConversation() {
		if (currentConversation >= conversations.Count) {
			return conversations[conversations.Count-1].lines;
		}
		return conversations[currentConversation++].lines;
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["currentConversation"] = currentConversation;
	}

	[System.Serializable]
	public class Conversation {
		public List<DialogueLine> lines;
	}
}
