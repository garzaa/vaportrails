#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditorCameraFollower : MonoBehaviour {

    void OnEnable() {
        EditorApplication.update += Update;
    }

    void Update() {
        if (CanFollow()) {
            this.transform.position = (Vector2) SceneView.lastActiveSceneView.camera.transform.position;
        }
    }
    
    bool CanFollow() {
        return (
            !Application.isPlaying
            && Camera.current != null
            && Camera.current.transform != null
        );
    }

    void OnDisable() {
        EditorApplication.update -= Update;
        this.transform.position = Vector2.zero;
    }

}

# endif
