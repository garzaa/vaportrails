using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rps;
    public bool unscaled;
    public bool resetOnDisable = true;

    float lastUpdate = 0f;

    Animator animator;

    void Start() {
        animator = GetComponentInParent<Animator>();
    }

    void LateUpdate() {
        float t = unscaled ? Time.unscaledTime : Time.time;

        // animator speed added for entity hitstop
        if (t > lastUpdate && (animator!=null && animator.speed > 0)) {
            Vector3 r = transform.localRotation.eulerAngles;
            r.z = ((rps * t * 360)) % 360;
            transform.localRotation = Quaternion.Euler(r);

            lastUpdate = t;
        }
    }

    void OnDisable() {
        transform.localRotation = Quaternion.identity;
    }
}
