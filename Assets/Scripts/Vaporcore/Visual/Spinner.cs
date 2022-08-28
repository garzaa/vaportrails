using UnityEngine;

public class Spinner : MonoBehaviour {
    public float speed;
    public bool unscaled;

    float lastUpdate = 0f;

    void LateUpdate() {
        float t = unscaled ? Time.unscaledTime : Time.time;

        if (t > lastUpdate) {
            Vector3 r = transform.localRotation.eulerAngles;
            r.z = ((speed * t)) % 360;
            transform.localRotation = Quaternion.Euler(r);

            lastUpdate = t;
        }
    }
}
