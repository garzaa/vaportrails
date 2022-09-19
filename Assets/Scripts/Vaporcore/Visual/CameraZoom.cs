using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

public class CameraZoom : MonoBehaviour {
	PixelPerfectCamera pixelCamera;
	public GameObject speedLines;

	int x;
	int y;

	Coroutine zoomRoutine;

	void Start() {
		pixelCamera = GetComponent<PixelPerfectCamera>();
		x = pixelCamera.refResolutionX;
		y = pixelCamera.refResolutionY;
	}

	public void Zoom(int level) {
		pixelCamera.refResolutionX = x / level;
		pixelCamera.refResolutionY = y / level;
		speedLines.SetActive(true);
	}

	public void ResetZoom() {
		speedLines.SetActive(false);
		pixelCamera.refResolutionX = x;
		pixelCamera.refResolutionY = y;
	}

	public void ZoomFor(int level, float duration) {
		if (zoomRoutine != null) StopCoroutine(zoomRoutine);
		Zoom(level);
		zoomRoutine = StartCoroutine(UnZoom(duration));
	}

	IEnumerator UnZoom(float duration) {
		yield return new WaitForSecondsRealtime(duration);
		ResetZoom();
	}
}
