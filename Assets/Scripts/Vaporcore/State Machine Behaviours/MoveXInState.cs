using UnityEngine;

public class MoveXInState : RigidBodyAffector {
    public float speed;
    public bool pickMax = false;
    public bool onEnter = false;
    public bool onUpdate = true;

    Entity entity;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        entity = animator.GetComponent<Entity>();
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    override protected void Enter() {
        if (!onEnter && !onUpdate) Debug.LogWarning("brainlet alert");
        if (onEnter) Move();
    }

    override protected void Update() {
        if (onUpdate) Move();
    }

    void Move() {
        Vector2 v = rb2d.velocity;
        if (pickMax) {
            float entityRelativeSpeed = v.x*entity.Forward();
            if (speed < 0 ) {
                v.x = Mathf.Min(speed, entityRelativeSpeed);
            } else {
                v.x = Mathf.Max(speed, entityRelativeSpeed);
            }
            v.x *= entity.Forward();
        } else {
            v.x = speed * entity.Forward();
        }
        rb2d.velocity = v;
    }
}
