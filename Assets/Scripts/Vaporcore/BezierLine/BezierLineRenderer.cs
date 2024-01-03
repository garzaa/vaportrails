using System.Collections.Generic;
using UnityEngine;

// Bezier Curve Line Renderer for Unity, credit goes to World of Zero: https://www.youtube.com/watch?v=tgCFzoG_BJM

[ExecuteInEditMode]
public class BezierLineRenderer : MonoBehaviour {
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public LineRenderer lineRenderer;
    public int vertexCount = 12;

	public bool animated = false;

	void Start() {
		Render();		
	}

    void Update() {
		if (animated || (Application.isEditor && !Application.isPlaying)) {
			Render();
		}
    }

	void Render() {
		if (!(point1 && point2 && point3 && lineRenderer)) return;

		var pointList = new List<Vector3>();
		for (float ratio = 0; ratio <= 1; ratio += 1.0f / vertexCount)
		{
			var tangentLineVertex1 = Vector3.Lerp(point1.position, point2.position, ratio);
			var tangentLineVertex2 = Vector3.Lerp(point2.position, point3.position, ratio);
			var bezierpoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
			pointList.Add(bezierpoint);
		}
		lineRenderer.positionCount = pointList.Count;
		lineRenderer.SetPositions(pointList.ToArray());
	}

    private void OnDrawGizmos() {
		if (!(point1 && point2 && point3 && lineRenderer)) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(point1.position, point2.position);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(point2.position, point3.position);
    }
}
