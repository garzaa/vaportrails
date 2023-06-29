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

    float texturePPU = 0.5f;
    float updateInterval = 0.2f;

	Color transparent;

    void ResetMap() {
        Color32[] colors = new Color32[fog.width*fog.height];
        for (int i=0; i<colors.Length; i++) {
            colors[i] = new Color(0, 0, 0, 1);
        }
        fog.SetPixels32(colors);
        fog.Apply();
    }

    void Start() {
		transparent = new Color32(0, 0, 0, 0);
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
		// wait for any transition stuff to move the player when the level starts
        // and then the camera
		yield return new WaitForSeconds(0.5f);

		for (;;) {
			Vector2 pos = cameraTarget.transform.position;
			pos *= texturePPU;
			pos += new Vector2(fog.width/2, fog.height/2);

			/*
			reveal in a 3-block cross like this
			 #
			###
             #
			*/

			int startX = Mathf.RoundToInt(pos.x) - 1;
			int startY = Mathf.RoundToInt(pos.y) - 1;

			for (int x = startX; x <= startX+2; x++) {
				for (int y = startY; y <= startY+2; y++) {
					if ((x-startX % 2 == 0) && (y-startY % 2) == 0) {
						continue;
					}
					fog.SetPixel(x, y, transparent);
				}
			}
			fog.Apply();

			yield return new WaitForSeconds(updateInterval);
		}
    }
}
