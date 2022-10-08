using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConversationContainer : SavedObject {
	#pragma warning disable 0649
	[SerializeField] List<Conversation> conversations = new List<Conversation>();
	#pragma warning restore 0649

	int currentConversation = 0;

	bool _unread;

	public bool unread => _unread || currentConversation<conversations.Count-1;

	override protected void Initialize() {
		_unread = true;
	}

	protected override void LoadFromProperties() {
		_unread = Get<bool>("unread");
		currentConversation = Get<int>("currentConversation");
	}

	public List<DialogueLine> GetNextConversation() {
		_unread = false;
		if (currentConversation == conversations.Count-1) {
			return conversations[currentConversation].lines;
		}
		return conversations[currentConversation++].lines;
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["unread"] = unread;
		properties["currentConversation"] = currentConversation;
	}

	[System.Serializable]
	public class Conversation {
		public List<DialogueLine> lines;
	}
}
