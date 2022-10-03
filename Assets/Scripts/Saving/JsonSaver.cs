using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public class JsonSaver {
    const string folder = "saves";
    const string extension = ".json";

    public void SaveFile(Save save, int slot) {
        using (StreamWriter jsonWriter = new StreamWriter(GetSavePath(slot), append: false)) {
            jsonWriter.Write(JsonConvert.SerializeObject(save, Formatting.Indented));
        }
    }

    public Save LoadFile(int slot) {
        Save save;
        using (StreamReader r = new StreamReader(GetSavePath(slot))) {
            string fileJson = r.ReadToEnd();
            save = JsonConvert.DeserializeObject<Save>(fileJson);
        }
        return save;
    }

    public bool HasFile(int slot) {
        if (!File.Exists(GetSavePath(slot))) return false;
        try {
            if (!CompatibleVersions(slot)) {
                return false;
            }
            return true;
        } catch (Exception) {
            // deal with legacy saves/changed formats
            return false;
        }
    }
    
    string GetSavePath(int slot) {
        if (!Directory.Exists(GetFolderPath())) {
            Directory.CreateDirectory(GetFolderPath());
        }
        return Path.Combine(GetFolderPath(), slot+extension);
    }

    string GetFolderPath() {
        return Path.Combine(Application.persistentDataPath, folder);
    }

    public bool CompatibleVersions(int saveSlot) {
        string version;

        using (StreamReader r = new StreamReader(GetSavePath(saveSlot))) {
            string fileJson = r.ReadToEnd();
            version = JObject.Parse(fileJson)["version"].ToString();
        }

        string[] saveVersion = version.Split('.');
        string[] currentVersion = Application.version.Split('.');

        return saveVersion[0].Equals(currentVersion[0]);
    }
}