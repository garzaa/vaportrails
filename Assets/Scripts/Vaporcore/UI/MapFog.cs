using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MapFog : MonoBehaviour {
    #pragma warning disable 0649
    [SerializeField] Texture2D fog;
    [SerializeField] GameObject cameraTarget;
    #pragma warning restore 0649

    float texturePPU = 0.5f;
    float updateInterval = 0.2f;

	Color transparent;
    SaveManager saveManager;

    void ResetMap() {
        Color32[] colors = new Color32[fog.width*fog.height];
        for (int i=0; i<colors.Length; i++) {
            colors[i] = new Color(0, 0, 0, 1);
        }
        fog.SetPixels32(colors);
        fog.Apply();
    }

    void Start() {
        saveManager = GameObject.FindObjectOfType<SaveManager>();
		transparent = new Color32(0, 0, 0, 0);
		ResetMap();
       	StartCoroutine(MapUpdateRoutine()); 
    }

    void LoadIfPossible() {
        // if fog on disk, overwrite what's currently in memory
        if (File.Exists(SavedImageName())) {
            using (FileStream fileStream = File.Open(SavedImageName(), FileMode.Open)) {
                BinaryFormatter bf = new BinaryFormatter();
                bool result = fog.LoadImage((byte[]) bf.Deserialize(fileStream));
                if (!result) {
                    Debug.LogWarning("failed to load mapfog at "+SavedImageName());
                }
            }
        }
    }

    public void Save() {
        // save a.png of [area name] map fog.png to the save directory
        using (FileStream fileStream = File.Create(SavedImageName())) {
            byte[] imageBytes = fog.EncodeToPNG();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fileStream, imageBytes);
        }
    }

    string SavedImageName() {
        return Path.Combine(
            saveManager.GetSaveFolderPath(),
            SceneManager.GetActiveScene().name+" MapFog.png"
        );
    }

    IEnumerator MapUpdateRoutine() {
        // load map from disk if possible
        LoadIfPossible();

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
