using UnityEngine;
using UnityEngine.EventSystems;

public class SoundOnClick: MonoBehaviour, IPointerDownHandler {
	public AudioResource onClick;

	public void OnPointerDown(PointerEventData data) {
		onClick.PlayFrom(gameObject);
	}
}
