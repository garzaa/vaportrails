using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class StreakRenderer : LineRendererEditor {
    public Transform start;
    public Transform end;

    public bool useWorldSpace = false;
    public bool continuous = true;

    override protected void Start() {
        base.Start();
        line.positionCount = 2;
        line.useWorldSpace = useWorldSpace;
        SetPoints();
    }

    void Update() {
       if (continuous || Application.isEditor) SetPoints();
    }

    void LateUpdate() {
        if (continuous) SetPoints();
    }

    void SetPoints() {
        if (start == null || end == null) return;
        Vector3[] points = new Vector3[2];
        points[0] = useWorldSpace ? start.position : start.localPosition;
        points[1] = useWorldSpace ? end.position : end.localPosition;
        line.SetPositions(points);
    }
}
