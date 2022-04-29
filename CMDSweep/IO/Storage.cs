using CMDSweep.Data;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CMDSweep.IO;

static class Storage
{
    private const string SaveFilePath = "save.json";
    private const string SettingsFilePath = "Assets/settings.json";
    private const string HelpFilePath = "Assets/help.txt";

    internal static GameSettings LoadSettings()
    {
        string settingsText = File.ReadAllText(SettingsFilePath);

        GameSettings? settings = JsonConvert.DeserializeObject<GameSettings>(settingsText);

        if (settings == null)
            throw new Exception("Failed to load settings");

        return settings;
    }

    internal static SaveData LoadSaveFile(GameSettings settings)
    {
        SaveData? sd;
        if (File.Exists(SaveFilePath))
        {
            string saveText = File.ReadAllText(SaveFilePath);
            sd = JsonConvert.DeserializeObject<SaveData>(saveText);
        }
        else
        {
            sd = new(settings.DefaultDifficulties);
            WriteSave(sd);
        }

        if (sd == null)
            throw new Exception("Failed to open or storage file");

        return sd;
    }

    internal static string LoadHelpFile() => File.ReadAllText(HelpFilePath);

    internal static void WriteSave(SaveData sd)
    {
        string json = JsonConvert.SerializeObject(sd);
        File.WriteAllText(SaveFilePath, json);
    }
}
