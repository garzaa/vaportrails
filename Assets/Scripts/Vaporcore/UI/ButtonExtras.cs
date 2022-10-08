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

	public UnityEvent onHover;

	public void OnPointerEnter(PointerEventData d) {
		hoverSound?.PlayFrom(transform.root.gameObject);
		onHover.Invoke();
	}

	public void OnSelect(BaseEventData e) {
		hoverSound?.PlayFrom(transform.root.gameObject);
		onHover.Invoke();
	}

	public void OnPointerDown(PointerEventData d) {
		clickSound.PlayFrom(transform.root.gameObject);
	}


	public void OnSubmit(BaseEventData d) {
		clickSound.PlayFrom(transform.root.gameObject);
	}
}
