using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonMotion : MonoBehaviour  {

    public int fps = 18;
    public List<GameObject> ignoreGameobjects;

    float lastUpdateTime = 0f;
    bool forceUpdateThisFrame = false;
    List<Snapshot> snapshots = new List<Snapshot>();

    void Start() {
        CreateTargetList(this.transform);
    }

    void CreateTargetList(Transform parent) {
        if (parent == null) return;
        int childrenCount = parent.childCount;

        for (int i = 0; i < childrenCount; i++) {
            Transform target = parent.GetChild(i);
            // yeah this is slow. too bad, it runs once at startup
            if (!ignoreGameobjects.Contains(target.gameObject)) {
                snapshots.Add(new Snapshot(target));
            }
            CreateTargetList(target);
        }
    }

    void LateUpdate() {
        if (forceUpdateThisFrame || Time.unscaledTime - lastUpdateTime > 1f/this.fps) {
            foreach (Snapshot s in snapshots) {
                s.UpdateSelf();
            }
            this.lastUpdateTime = Time.unscaledTime;
            forceUpdateThisFrame = false;
        } else {
            foreach (Snapshot snapshot in snapshots) {
                snapshot.Maintain();
            }
        }
    }

    public void ForceUpdate() {
        forceUpdateThisFrame = true;
    }

    private class Snapshot {
        public Transform transform;
        public Vector3 position;
        public Quaternion rotation;

        public Snapshot(Transform transform) {
            this.transform = transform;
            this.UpdateSelf();
        }

        public void UpdateSelf() {
            position = transform.localPosition;
            rotation = transform.localRotation;
        }

        public void Maintain() {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }
    }
}
