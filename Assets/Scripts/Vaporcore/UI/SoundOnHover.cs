using UnityEngine;
using UnityEngine.EventSystems;

public class SoundOnHover : MonoBehaviour, IPointerEnterHandler {
	public AudioResource onHover;

	public void OnPointerEnter(PointerEventData data) {
		onHover.PlayFrom(gameObject);
	}
}
