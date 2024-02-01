using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Threading.Tasks;

public class JsonSaver {
    const string extension = ".json";
    // used in an ifndef statement, dw future me
    readonly string persistentDataPath;
    const string folder = "saves";

    public JsonSaver(string datapath) {
        persistentDataPath = datapath;
    }

    public async void SaveFile(Save save, int slot) {
        string saveString = JsonConvert.SerializeObject(save, Formatting.Indented);
        string filePath = GetSavePath(slot);
        #if (STEAM || EDITOR_STEAM)
            // Debug.Log("Writing remote Steam file at path "+filePath);
            Steamworks.SteamRemoteStorage.FileWrite(filePath, Encoding.UTF8.GetBytes(saveString));
        #else
            using StreamWriter jsonWriter = new StreamWriter(filePath, append: false);
		    jsonWriter.Write(saveString);
        #endif
        await Task.Yield();
	}

    public Save LoadFile(int slot) {
        string filePath = GetSavePath(slot);
        string fileJson;
        #if STEAM || EDITOR_STEAM
            fileJson = Encoding.UTF8.GetString(Steamworks.SteamRemoteStorage.FileRead(filePath));
            // Debug.Log("Read Steam cloud save at "+filePath);
        #else
            using (StreamReader r = new StreamReader(filePath)) {
                fileJson = r.ReadToEnd();
            }
        #endif
        return JsonConvert.DeserializeObject<Save>(fileJson);
    }

    public bool HasFile(int slot) {
        string filePath = GetSavePath(slot);
        #if STEAM || EDITOR_STEAM
            if (!Steamworks.SteamRemoteStorage.FileExists(filePath)) {
                // Debug.Log("No remote Steam file at path "+filePath);
                return false;
            }
        # else
            if (!File.Exists(filePath)) return false;
        # endif

        try {
            return CompatibleVersions(slot);
        } catch (Exception) {
            return false;
        }
    }
    
    string GetSavePath(int slot) {
        return Path.Combine(GetFolderPath(slot), slot+extension);
    }

    public string GetFolderPath(int slot) {
        string folderPath;

        #if STEAM || EDITOR_STEAM
            folderPath = "";
        # else
            folderPath = Path.Combine(persistentDataPath, folder, slot.ToString());
            Directory.CreateDirectory(folderPath);
        # endif 
        return folderPath;
    }

    public bool CompatibleVersions(int saveSlot) {
        string version = LoadFile(saveSlot).version;

        string[] saveVersion = version.Split('.');
        string[] currentVersion = Application.version.Split('.');

        return saveVersion[0].Equals(currentVersion[0]) && saveVersion[1].Equals(currentVersion[1]);
    }
}
