using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour {
 
    public Vector2 speed;

	private Transform cameraTransform;
	private Vector3 previousCameraPosition;
	private bool activeLastFrame;

	bool started = false;
	Vector3 distance;

	void Start() {
		GameObject gameCamera = GameObject.Find("Main Camera");
		if (gameCamera == null) return;
		cameraTransform = gameCamera.transform;
		previousCameraPosition = Vector2.zero;
		RoundChildren(this.transform);
		transform.localPosition = Vector2.zero;
		started = true;
	}

	void OnEnable() {
		if (!started) return;
		else Start();
	}

	public void LateUpdate() {
		if (cameraTransform == null ) {
			return;
		}

		Move();
		PixelPerfectClamp(transform.position, 16);
		previousCameraPosition = cameraTransform.position;
	}

	// prevent pixel jittering
    void RoundChildren(Transform t) {
		t.position = t.position.Round(2);
        foreach (Transform child in t) {
            child.position = child.position.Round(2);
			RoundChildren(child);
        }
    }

	void Move() {
		distance = cameraTransform.position - previousCameraPosition;
		transform.localPosition += Vector3.Scale(distance, speed);
	}

	private Vector3 PixelPerfectClamp(Vector3 locationVector, float pixelsPerUnit) {
		Vector3 vectorInPixels = new Vector3(Mathf.CeilToInt(locationVector.x * pixelsPerUnit), Mathf.CeilToInt(locationVector.y * pixelsPerUnit), Mathf.CeilToInt(locationVector.z * pixelsPerUnit));
		return vectorInPixels / pixelsPerUnit;
	}
}
