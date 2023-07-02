using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonExtras : MonoBehaviour, 
	IPointerEnterHandler,
	ISelectHandler,
	ISubmitHandler,
	IPointerDownHandler {
	
	public AudioResource hoverSound;
	public AudioResource clickSound;

	public UnityEvent onSelect;
	public bool includeHover = true;

	public void OnPointerEnter(PointerEventData d) {
		if (!includeHover) return;

		hoverSound?.PlayFrom(transform.root.gameObject);
		onSelect.Invoke();
	}

	public void OnSelect(BaseEventData e) {
		hoverSound?.PlayFrom(transform.root.gameObject);
		onSelect.Invoke();
	}

	public void OnPointerDown(PointerEventData d) {
		clickSound?.PlayFrom(transform.root.gameObject);
	}

	public void OnSubmit(BaseEventData d) {
		clickSound?.PlayFrom(transform.root.gameObject);
	}
}
