using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

[RequireComponent(typeof(CanvasScaler))]
public class DynamicCanvasScaler : MonoBehaviour {
    static int pixelScale = 1;
    Canvas targetCanvas;

    public float multiplier = 1;
    public int minimumScale = 1;
	int minScreenHeight;

    void Start() {
        targetCanvas = GetComponent<Canvas>();
        minScreenHeight = FindObjectOfType<PixelPerfectCamera>().refResolutionY * FindObjectOfType<CameraZoom>().GetZoomLevel();
    }

    public void ForceUpdate() {
        Start();
    }

    void LateUpdate() {
        pixelScale = ComputePixelScale();
        targetCanvas.scaleFactor = Mathf.Max(1, pixelScale * multiplier);
    }

    int ComputePixelScale() {
        return Mathf.Max(minimumScale, Mathf.FloorToInt((float)Screen.height / minScreenHeight));
    }

    public static int GetPixelScale() {
        return pixelScale;
    }
}
