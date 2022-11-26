using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventOnInteract : Interactable {
	public UnityEvent InteractEvent;

	public override void OnInteract(EntityController player) {
		InteractEvent.Invoke();
	}

}
