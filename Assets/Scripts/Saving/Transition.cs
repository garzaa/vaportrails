using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Transition", menuName = "Data/Runtime/Transition")]
public class Transition : ScriptableObject {
    public SubwayTransition subway = null;
    public PlayerPositionTransition position = null;
    public Beacon beacon = null;

    public void Clear() {
        subway = null;
        position = null;
        beacon = null;
    }

    public bool IsEmpty() {
        return !(subway || position || beacon);
    }

    #if UNITY_EDITOR
    void OnEnable() {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }
    #endif

    void OnPlayModeChanged(PlayModeStateChange change) {
        if (!EditorApplication.isPlaying) Clear();
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
