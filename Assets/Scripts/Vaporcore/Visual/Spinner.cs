using UnityEngine;

public class Spinner : MonoBehaviour {
    public float rps;
    public bool unscaled;
    public bool resetOnDisable = true;

    float lastUpdate = 0f;

    void LateUpdate() {
        float t = unscaled ? Time.unscaledTime : Time.time;

        if (t > lastUpdate) {
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
