using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class PalmTree : MonoBehaviour, IWindReceiver {
	public float tileHeight = 3;
	public int segmentsPerTile = 4;
	public Texture2D windTexture;
	public float windSpeed = 50;
	public float windSize = 1000;
	public float windStrength = 1.5f;
	[Range(-1, 1)] public float direction = -1;
	public GameObject treetop;


	LineRenderer lineRenderer;

	Vector3[] points;

	void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.useWorldSpace = false;
		RegenerateTrunk();
	}

	public void Wind(float speed, float size, float strength, float dir) {
		windSpeed = speed;
		windSize = size;
		windStrength = strength;
		direction = dir;
	}

	void Update() {
		// get positions, store them in points
		lineRenderer.GetPositions(points);
		Vector2 noiseCoord;
		float offset;
		float a = 1/segmentsPerTile;
		float c;
		float heightLoss = 0;
		for (int i=0; i<points.Length; i++) {
			// right at the base, no distortion
			if (i <= 2) {
				points[i].x = 0;
				continue;
			}

			// look up distortion X via the noise texture
			noiseCoord = points[i] + transform.position;
			noiseCoord += Vector2.one * windSpeed * Time.time;
			noiseCoord /= windSize;
			offset = windTexture.GetPixelBilinear(noiseCoord.x, noiseCoord.y).r;
			// map from 0 1 to -1 1
			offset = offset*2 - 1;
			// convert to direction
			offset += direction;

			// add strength;
			offset *= windStrength;

			// don't distort at the base
			float ratio = ((float) i-2) / points.Length;
			offset *= ratio;

			points[i].x = offset;
			// there needs to be some trig here
			// c2 = a2 + b2
			// c = sqrt(a2 + b2)
			c = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(offset, 2));
			// difference between adjacent and hypotenuse
			heightLoss = c - a;
		
			// except it can't actually be heightloss, it's heightloss times the ratio of offset to height
			// don't want to compute that again, so here
			points[i].y = GetSegmentPos(i) - (heightLoss * 0.125f);
		}
		lineRenderer.SetPositions(points);
		// then set the treetop position to the final point
		Vector3 topPos = lineRenderer.GetPosition(points.Length-1);
		treetop.transform.localPosition = topPos;
		treetop.transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(transform.position, topPos, Vector3.forward) - 45);
	}

	// line renderer
	// offset vertices based on noise texture lookup
	[Button("Regenerate")]
	private void RegenerateTrunk() {
		points = new Vector3[Mathf.CeilToInt(segmentsPerTile * tileHeight)];
		lineRenderer.positionCount = points.Length;
		for (int i=0; i<points.Length; i++) {
			points[i] = new Vector3(
				0,
				GetSegmentPos(i),
				0
			);
		}
		lineRenderer.SetPositions(points);

		// then set the shader values
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		lineRenderer.GetPropertyBlock(block);
		block.SetFloat("_TrunkHeight", tileHeight);
		lineRenderer.SetPropertyBlock(block);
	}

	float GetSegmentPos(int pointNum) {
		return ((float) pointNum / segmentsPerTile);
	}
}
