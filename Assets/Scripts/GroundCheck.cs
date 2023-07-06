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

    const float groundCastLength = 0.5f;
    const float maxClimbAngle = 45;

    Rigidbody2D rb2d;

    void Awake() {
        col = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();
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

        groundData.grounded = grounded;// && (Time.time-groundData.jumpTime > 0.2f);
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
            1f,
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
        Vector2 start = new Vector2(transform.position.x, transform.position.y);
        
        // cast forward from just above the bottom point
        // to see if they're moving up onto a slope
        RaycastHit2D forwardHit = Physics2D.Raycast(
            start + (Vector2.down * (col.bounds.extents.y - 0.01f)),
            Vector2.left * transform.lossyScale.x,
            col.bounds.extents.x + Mathf.Abs(rb2d.velocity.x * Time.fixedDeltaTime),
            defaultLayerMask
        );

        Debug.DrawLine(start, start + Vector2.left * transform.lossyScale.x, Color.red);

        if (forwardHit && Vector2.Angle(Vector2.up, forwardHit.normal) < maxClimbAngle) {
            Debug.DrawLine(start, forwardHit.point, Color.yellow);
            return forwardHit.normal;
        }

        RaycastHit2D downHit = Physics2D.Raycast(
            start,
            Vector2.down,
            col.bounds.extents.y + groundCastLength,
            defaultLayerMask
        );


        if (downHit) {
            Debug.DrawLine(start, downHit.point, Color.blue);
            return downHit.normal;
        }

        return Vector2.up;
    }

    RaycastHit2D DefaultLinecast(Vector2 origin) {
        Vector2 start = origin + Vector2.up * 0.05f;
        Vector2 end = origin + (-currentNormal * 0.25f);

        Debug.DrawLine(start, end, Color.red);
        return Physics2D.Linecast(
            start,
            end,
            defaultLayerMask
        );
    }

    float GetGroundDistance() {
        // this has to start from the forward edge of the collider
        Vector2 start = transform.position;// new Vector2(col.bounds.max.x*transform.lossyScale.x, col.bounds.min.y);
        // then move it forward
        start += Vector2.right * transform.lossyScale.x * col.bounds.extents.x;
        RaycastHit2D hit = Physics2D.Raycast(
            start,
            Vector2.down,
            99,
            defaultLayerMask
        );
        if (hit) {
            Debug.DrawLine(start, hit.point, Color.green);
        }
        return hit ? hit.distance : 99;
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
    public float jumpTime = -10;
}
