using UnityEngine;

[ExecuteInEditMode]
public class Spinner : MonoBehaviour {
    public float rps;
    public bool unscaled;
    public bool resetOnDisable = true;

    float lastUpdate = 0f;

    public int fps = 0;

    Animator animator;

    void Start() {
        animator = GetComponentInParent<Animator>();
    }

    void LateUpdate() {
        float t = unscaled ? Time.unscaledTime : Time.time;

        // animator speed added for entity hitstop
        // WHY DOES THIS SPEED UP FUCK
        // fps limiter math is wrong
        // last update + (1/24)
        // no that works...
        bool canUpdate = (fps == 0) || (t > (lastUpdate + (1/((float)fps))));

        if (t > lastUpdate && canUpdate && !(animator!=null && animator.speed == 0)) {
            Vector3 r = transform.localRotation.eulerAngles;
            // change it to be additive instead of the full rotation
            r.z = ((rps * t * 360)) % 360;
            transform.localRotation = Quaternion.Euler(r);

            lastUpdate = t;
        }
    }

    void OnDisable() {
        transform.localRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);
    }
}
