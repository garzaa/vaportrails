using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScrollViewUtils : MonoBehaviour {
	bool scrolling = false;
	Vector2 targetPos;
	float lerpRate = 0.05f;
	RectTransform target;

	ScrollRect scrollView;

	void Awake() {
		scrollView = GetComponent<ScrollRect>();
	}

	public void ScrollToChild(RectTransform child) {
		Canvas.ForceUpdateCanvases();
		targetPos = new Vector2(
			scrollView.content.localPosition.x,
			GetSnapToPositionToBringChildIntoView(child).y
		);
		target = child;
		scrolling = true;
	}

	void Update() {
		if (Input.mouseScrollDelta.sqrMagnitude != 0 || Input.GetMouseButton(0)) {
			scrolling = false;
			return;
		}

		if (scrolling) {
			if (!target) {
				scrolling = false;
				return;
			}

			scrollView.content.localPosition = Vector2.Lerp(scrollView.content.localPosition, targetPos, lerpRate);

			if (Mathf.Abs(targetPos.y - scrollView.content.localPosition.y) < 1f) {
				scrollView.content.localPosition = targetPos;
				scrolling = false;
			}

		}
	}
	
	Vector2 GetSnapToPositionToBringChildIntoView(RectTransform child) {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = scrollView.viewport.localPosition;
        Vector2 childLocalPosition   = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        Canvas.ForceUpdateCanvases();
        return result;
    }
}
