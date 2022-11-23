using UnityEngine;

[CreateAssetMenu(fileName = "Transition", menuName = "Data/Runtime/Transition")]
public class Transition : ScriptableObject {
    public SubwayTransition subway;
    public PlayerPositionTransition position;

    public void Clear() {
        subway = null;
        position = null;
    }

    public bool IsEmpty() {
        return !(subway || position);
    }

    public class NullableTransitionValue {
        public static implicit operator bool(NullableTransitionValue instance) {
            return instance != null;
        }
    }

    public class SubwayTransition : NullableTransitionValue {
        bool arrivingFromLeft = false;
    }

    public class PlayerPositionTransition : NullableTransitionValue {
        public Vector2 position;

        public PlayerPositionTransition(Vector2 v) {
            position = v;
        }
    }
}
