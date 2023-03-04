using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class GroundCheck : MonoBehaviour {
    public GroundData groundData = new GroundData();

    public UnityEvent onLedgeStep;

    [SerializeField] bool detecting = true;
    
    Collider2D col;

    int defaultLayerMask = Layers.GroundMask;

    RaycastHit2D leftHit;
    RaycastHit2D rightHit;
    bool leftGrounded, rightGrounded;
    Collider2D groundCollider;
    bool grounded;
    bool onLedge;
    Vector2 currentNormal = Vector2.up;
    Vector2 bottomCenter;
    GameObject currentGround;

    List<RaycastHit2D> platforms = new List<RaycastHit2D>();
    List<RaycastHit2D> nonPlatforms = new List<RaycastHit2D>();

    // avoid physics jank
    const float minHitInterval = 0.3f;
    float lastHitTime = -1000f;

    void Awake() {
        col = GetComponent<Collider2D>();
    }

    void FixedUpdate() {
        RefreshGroundData(groundData);

        leftHit = LeftGrounded();
        rightHit = RightGrounded();

        leftGrounded = leftHit.collider != null;
        rightGrounded = rightHit.collider != null;

        grounded = detecting && (leftGrounded || rightGrounded);
        onLedge = leftGrounded ^ leftGrounded;

        if (leftGrounded) groundCollider = leftHit.collider;
        else if (rightGrounded) groundCollider = rightHit.collider;
        else groundCollider = null;

        if (groundData.grounded && !grounded) {
            groundData.leftGround = true;
        } else if (!groundData.grounded && grounded) {
            if (Time.time-lastHitTime > minHitInterval) {
                groundData.hitGround = true;
                lastHitTime = Time.time;
            }
        }

        if (!groundData.onLedge && onLedge) {
            onLedgeStep.Invoke();
            groundData.ledgeStep = true;
        }

        groundData.platforms = TouchingPlatforms();

        groundData.grounded = grounded;
        groundData.onLedge = onLedge;

        currentNormal = GetGroundNormal();
        groundData.normal = currentNormal;
        groundData.normalRotation = Vector2.SignedAngle(Vector2.up, currentNormal);
        groundData.distance = GetGroundDistance();

        if (groundCollider != null) {
            groundData.groundObject = groundCollider.gameObject;
            groundData.groundCollider = groundCollider;
        }
    }

    List<RaycastHit2D> TouchingPlatforms() {
        platforms.Clear();

        platforms.AddRange(GetPlatforms(col.BottomLeftCorner()));
        platforms.AddRange(GetPlatforms(col.BottomRightCorner()));

        nonPlatforms.Clear();

        for (int i=0; i<platforms.Count; i++) {
            if (!platforms[i].collider.CompareTag(Tags.Platform)) {
                nonPlatforms.Add(platforms[i]);
            }
        }

        for (int i=0; i<nonPlatforms.Count; i++) {
            platforms.Remove(nonPlatforms[i]);
        }

        return platforms;
    }

    RaycastHit2D[] GetPlatforms(Vector2 corner) {
        return Physics2D.CircleCastAll(
            corner,
            1.28f,
            Vector2.zero,
            0f,
            defaultLayerMask
        );
    }

    RaycastHit2D LeftGrounded() {
        return DefaultLinecast(col.BottomLeftCorner());
    }

    RaycastHit2D RightGrounded() {
        return DefaultLinecast(col.BottomRightCorner());
    }

    void RefreshGroundData(GroundData groundData) {
        groundData.groundCollider = null;
        groundData.groundObject = null;
        groundData.leftGround = false;
        groundData.hitGround = false;
        groundData.ledgeStep = false;
        groundData.distance = 99;
    }

    Vector2 GetGroundNormal() {
        Vector2 start = transform.position;
        Vector2 end = (Vector2) transform.position + Vector2.down*0.5f;

        RaycastHit2D hit = Physics2D.Linecast(
            start,
            end,
            defaultLayerMask
        );

        if (hit.transform != null) {
            Debug.DrawLine(start, hit.point, Color.red);
            return hit.normal;
        } else {
            Debug.DrawLine(start, end, Color.green);
            return Vector2.up;
        }
    }

    RaycastHit2D DefaultLinecast(Vector2 origin) {
        Vector2 start = origin + Vector2.up * 0.05f;
        Vector2 end = origin + (-currentNormal * 0.1f);

        Debug.DrawLine(start, end, Color.red);
        return Physics2D.Linecast(
            start,
            end,
            defaultLayerMask
        );
    }

    float GetGroundDistance() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 99, defaultLayerMask);
        return hit.transform ? hit.distance : 99;
    }

    public void DisableFor(float seconds) {
        StartCoroutine(WaitAndEnable(seconds));
    }

    IEnumerator WaitAndEnable(float seconds) {
        detecting = false;
        yield return new WaitForSeconds(seconds);
        detecting = true;
    }
}

[System.Serializable]
public class GroundData {
    public bool grounded;
    public bool onLedge;
    public bool leftGround;
    public bool hitGround;
    public bool ledgeStep;
    public Vector2 normal;
    public float normalRotation;
    public GameObject groundObject;
    public List<RaycastHit2D> platforms;
    public Collider2D groundCollider;
    public float distance = 99;
}
