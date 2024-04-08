#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
            // don't run this if in prefab mode (and then dirty it/make a save confirm dialog)
            && PrefabStageUtility.GetCurrentPrefabStage() == null
        );
    }

    void OnDisable() {
        EditorApplication.update -= Update;
        this.transform.position = Vector2.zero;
    }

}

# endif
