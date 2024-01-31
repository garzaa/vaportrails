using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class BarUI : MonoBehaviour {
    public RectTransform indicator;
    public RectTransform container;
    public RectTransform background;
    public RectTransform deltaIndicator;
    public float pixelsPerUnit;
	public bool normalizeSize = false;
	[ShowIf(nameof(normalizeSize))]
	public float size = 64;
	public bool disappearAfterDelta = false;
	const float disappearDelay = 3f;

    public int _max;
    public int _current;
    
    readonly float deltaDelay = 0.5f;
    readonly float deltaMoveSpeed = 20f;
    readonly float deltaTolerance = 1f;
    float currentDelta;
    float changeTime = -100;
	CanvasGroup canvasGroup;

    void OnEnable() {
        currentDelta = 0;
    }

	void Awake() {
        if (!disappearAfterDelta) return;
		canvasGroup = GetComponent<CanvasGroup>();
	}

    public int max {
        get { return _max; }
    }
    public int current {
        get { return _current; }
	}

    void Redraw() {
		if (normalizeSize && max > 0) {
            pixelsPerUnit = size / max;
        }

        if (background != null) ScaleImage(background, max);
        if (container != null) {
            ScaleImage(container, max);
        }
        ScaleImage(indicator, current);
		if (disappearAfterDelta && current!=max) {
            canvasGroup.alpha = 1;
        } else if (disappearAfterDelta) {
            canvasGroup.alpha = 0;
        }
    }

    void ScaleImage(RectTransform r, float val, int mod=0) {
        r.sizeDelta = new Vector2((val*pixelsPerUnit)+mod, r.sizeDelta.y);
    }

	public void SetCurrent(int value) {
        int oldCurrent = _current;
		_current = value;
		changeTime = Time.time;
		if (_current != oldCurrent) Redraw();
	}

	public void SetMax(int value) {
		_max = value;
		if (deltaIndicator != null ) {
			ScaleImage(deltaIndicator, max);
			currentDelta=max;
		}
		Redraw();
	}

    void Update() {
        if (deltaIndicator == null) {
            return;
        } else if (currentDelta < current) {
			// if it's going up, always snap
			currentDelta = current;
			return;
		}
        
        if (Mathf.Abs(currentDelta-current) < deltaTolerance) {
            currentDelta=current;
        } else if (Time.time > changeTime + deltaDelay) {
            float dir = Mathf.Sign(current - currentDelta);
            currentDelta += (deltaMoveSpeed*Time.deltaTime*dir);
        }

        ScaleImage(deltaIndicator, currentDelta);
		
		if (disappearAfterDelta && Time.time > changeTime+disappearDelay) {
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.1f);
		}
    }
}
