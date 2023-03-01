using UnityEngine;

public class RotateToVelocity : MonoBehaviour {
    public Rigidbody2D rb2d;
    // this is the default if entities are normally facing up
    public float offset = -90;

    void Start() {
        if (rb2d == null) {
            rb2d = GetComponent<Rigidbody2D>();
            if (!rb2d) rb2d = GetComponentInParent<Rigidbody2D>();
        }
    }

    void LateUpdate() {
        // if in hitstop
        if (rb2d.constraints == RigidbodyConstraints2D.FreezeAll) {
            return;
        }
        SetAngleForVelocity(rb2d.velocity);
    }

    public void SetAngleForVelocity(Vector2 v) {
        this.transform.eulerAngles = new Vector3(
            0,
            0,
            Vector2.SignedAngle(
                Vector2.right,
                v
            ) + offset
        );
    }

    void OnDisable() {
        transform.rotation = Quaternion.identity;
    }
}
