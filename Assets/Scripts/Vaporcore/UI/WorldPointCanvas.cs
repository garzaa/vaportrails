using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldPointCanvas : MonoBehaviour {

    new Camera camera;
    public Vector2 position;
    public GameObject optionalAnchor;
    RectTransform r;
    Vector3 currentVelocity;
    float maxSpeed = 1000;
    public float smoothTime;
    Rigidbody2D rb2d;

    void Start() {
        camera = Camera.main;
        r = GetComponent<RectTransform>();
        Debug.Log(camera);
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
        r.position = Vector3.SmoothDamp(r.position, camera.WorldToScreenPoint(position), ref currentVelocity, smoothTime, maxSpeed);
        // r.position = camera.WorldToScreenPoint(position);
    }
}
