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
        public SceneReference scene;
        public float xOffset = 0;
        public string previousScenePath;
    }

    public class PlayerPositionTransition : NullableTransitionValue {
        public Vector2 vec2;

        public PlayerPositionTransition(Vector2 v) {
            vec2 = v;
        }
    }
}
