using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

public class MapFog : MonoBehaviour {
    #pragma warning disable 0649
    [SerializeField] Texture2D fog;
    [SerializeField] GameObject cameraTarget;
    #pragma warning restore 0649

    float texturePPU = 0.5f;
    float updateInterval = 0.2f;

	Color transparent;
    string sceneName;

    bool saving = false;

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
       	StartCoroutine(MapUpdateRoutine());
        sceneName = SceneManager.GetActiveScene().name;
    }

    void LoadIfPossible() {
        #if STEAM || EDITOR_STEAM
            if (Steamworks.SteamRemoteStorage.FileExists(SavedImageName())) {
                fog.LoadImage(Steamworks.SteamRemoteStorage.FileRead(SavedImageName()));
                // Debug.Log("loaded fog at path  "+SavedImageName());
            }
        #else
            if (File.Exists(SavedImageName())) {
                fog.LoadImage(File.ReadAllBytes(SavedImageName()));
            }
        #endif
    }

    public async void Save() {
        saving = true;
        // save a.png of [area name] map fog.png to the save directory
        byte[] imageBytes = fog.EncodeToPNG();
        string filePath = SavedImageName();
        #if STEAM || EDITOR_STEAM
            Steamworks.SteamRemoteStorage.FileWrite(filePath, imageBytes);
            // Debug.Log("saved fog at path  "+SavedImageName());
        # else
            File.WriteAllBytes(SavedImageName(), imageBytes);
        #endif
        saving = false;
        await Task.Yield();
    }

    string SavedImageName() {
        string imagePath;
        #if STEAM || EDITOR_STEAM
            imagePath = saveManager.GetSlot()+"_"+sceneName+" Map Fog.png";
        # else
            imagePath = Path.Combine(
                SaveManager.GetSaveFolderPath(),
                sceneName+" Map Fog.png"
            );
        #endif
        return imagePath;
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
			reveal in a 4-block cross like this
			 ##
			####
             ##
			*/

			int startX = Mathf.FloorToInt(pos.x) - 1;
			int startY = Mathf.FloorToInt(pos.y) - 1;

			for (int x = startX; x <= startX+3; x++) {
				for (int y = startY; y <= startY+3; y++) {
					if ((x-startX % 3 == 0) && (y-startY % 3) == 0) {
						continue;
					}
					fog.SetPixel(x, y, transparent);
				}
			}

			if (!saving) fog.Apply();

			yield return new WaitForSeconds(updateInterval);
		}
    }
}
