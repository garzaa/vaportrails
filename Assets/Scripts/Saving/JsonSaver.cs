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
        using StreamWriter jsonWriter = new StreamWriter(filePath, append: false);
        jsonWriter.Write(saveString);
        await Task.Yield();
	}

    public Save LoadFile(int slot) {
        string filePath = GetSavePath(slot);
        string fileJson;
        using (StreamReader r = new StreamReader(filePath)) {
            fileJson = r.ReadToEnd();
        }
        return JsonConvert.DeserializeObject<Save>(fileJson);
    }

    public bool HasFile(int slot) {
        string filePath = GetSavePath(slot);
        if (!File.Exists(filePath)) return false;

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
        folderPath = Path.Combine(persistentDataPath, folder, slot.ToString());
        Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    public bool CompatibleVersions(int saveSlot) {
        string version = LoadFile(saveSlot).version;

        string[] saveVersion = version.Split('.');
        string[] currentVersion = Application.version.Split('.');

        return saveVersion[0].Equals(currentVersion[0]) && saveVersion[1].Equals(currentVersion[1]);
    }
}
