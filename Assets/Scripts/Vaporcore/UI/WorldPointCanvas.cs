using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldPointCanvas : MonoBehaviour {

    new Camera camera;
    public Vector2 position;

    [Tooltip("Use this instead of writing a vector2 position.")]
    public GameObject optionalAnchor;
    RectTransform r;
    Vector3 currentVelocity;
    float maxSpeed = 1000;
    public float smoothTime;
    Rigidbody2D rb2d;

    public bool clampToScreen = false;
    Vector2 newPos = Vector2.zero;

    void Start() {
        camera = Camera.main;
        r = GetComponent<RectTransform>();
        if (optionalAnchor) {
            rb2d = optionalAnchor.GetComponent<Rigidbody2D>();
        }
    }

    void LateUpdate() {
        if (optionalAnchor) {
            if (rb2d) {
                position = rb2d.position;
            } else {
                position = optionalAnchor.transform.position;
            }
        }

        if (smoothTime >= 0) {
            r.position = Vector3.SmoothDamp(r.position, camera.WorldToScreenPoint(position), ref currentVelocity, smoothTime, maxSpeed);
        } else {
            r.position = camera.WorldToScreenPoint(position);
        }

        if (clampToScreen) {
            newPos.x = Mathf.Clamp(r.position.x, r.sizeDelta.x, Screen.width - r.sizeDelta.x);
            newPos.y = Mathf.Clamp(r.position.y, r.sizeDelta.y, Screen.height - r.sizeDelta.y);
            r.position = newPos;
        }
    }
}
