using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;

public class MapFog : MonoBehaviour {
    #pragma warning disable 0649
    [SerializeField] Texture2D fog;
    [SerializeField] GameObject cameraTarget;
    #pragma warning restore 0649

    float texturePPU = 0.125f;
    float updateInterval = 0.2f;

    void ResetMap() {
        Color32[] colors = new Color32[fog.width*fog.height];
        for (int i=0; i<colors.Length; i++) {
            colors[i] = new Color(0, 0, 0, 1);
        }
        fog.SetPixels32(colors);
        fog.Apply();
    }

    void Start() {
		ResetMap();
       	StartCoroutine(UpdateMap()); 
    }

    string EncodeMap() {
        Color32[] pixels = fog.GetPixels32();
        StringBuilder alphas = new StringBuilder(new string('0', pixels.Length));
        for (int i=0; i<pixels.Length; i++) {
            alphas[i] = (pixels[i].a > 0 ? '1' : '0');
        }
        return alphas.ToString();
    }

    void DecodeAndUpdateMap(string map) {
        Color32[] colors = new Color32[map.Length];
        for (int i=0; i<map.Length; i++) {
            colors[i] = new Color(0, 0, 0, (int) char.GetNumericValue(map[i]));
        }
        fog.SetPixels32(colors);
        fog.Apply();
    }

    IEnumerator UpdateMap() {
		for (;;) {
			Vector2 pos = cameraTarget.transform.position;
			pos *= texturePPU;
			pos += new Vector2(fog.width/2, fog.height/2);

			fog.SetPixel(
				Mathf.FloorToInt(pos.x),
				Mathf.FloorToInt(pos.y),
				new Color32(0, 0, 0, 0)
			);
			fog.Apply();

			yield return new WaitForSeconds(updateInterval);
		}
    }
}
