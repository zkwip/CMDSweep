using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

namespace CMDSweep;

internal static class Storage
{
    private const string SaveFilePath = "save.json";
    private const string SettingsFilePath = "settings.json";
    internal static GameSettings LoadSettings()
    {
        string settingsText = File.ReadAllText(SettingsFilePath);
        GameSettings? settings = JsonConvert.DeserializeObject<GameSettings>(settingsText);
        if (settings == null) throw new Exception("Failed to load settings");
        return settings;
    }
    internal static SaveData LoadSaveFile(GameSettings settings)
    {
        SaveData? sd = null;
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

    internal static void WriteSave(SaveData sd)
    {
        string json = JsonConvert.SerializeObject(sd);
        File.WriteAllText(SaveFilePath, json);
    }
}
internal class GameSettings
{
    public List<Difficulty> DefaultDifficulties;
    public Dictionary<string, ConsoleColor> Colors;
    public Dictionary<string, string> Texts;
    public Dictionary<string, int> Dimensions;
    public Dictionary<InputAction, List<ConsoleKey>> Controls;

    public StyleData GetStyle(string handle) => new(Colors[handle + "-fg"], Colors[handle + "-bg"]);
    public StyleData GetStyle(string fg, string bg) => new(Colors[fg], Colors[bg]);
}

internal class SaveData
{
    public List<Difficulty> Difficulties;
    public Difficulty CurrentDifficulty;
    public SaveData() { }

    public SaveData(List<Difficulty> difficulties)
    {
        Difficulties = new List<Difficulty>(difficulties);
        CurrentDifficulty = Difficulties[0];
    }
}

internal class HighscoreRecord
{
    public string Name;
    public TimeSpan Time;
    public DateTime Date;
}

internal class Difficulty
{
    public string Name;

    public int Width;
    public int Height;
    public int Mines;
    public int Lives;
    public int Safezone;
    public int DetectionRadius;

    public bool FlagsAllowed;
    public bool QuestionMarkAllowed;
    public bool WrapAround;
    public bool SubtractFlags;
    public bool OnlyShowAtCursor;
    public bool AutomaticDiscovery;

    public List<HighscoreRecord> Highscores;

    public override bool Equals(object? obj)
    {
        return obj is Difficulty difficulty &&
               Name == difficulty.Name &&
               Width == difficulty.Width &&
               Height == difficulty.Height &&
               Mines == difficulty.Mines &&
               Lives == difficulty.Lives &&
               Safezone == difficulty.Safezone &&
               DetectionRadius == difficulty.DetectionRadius &&
               FlagsAllowed == difficulty.FlagsAllowed &&
               QuestionMarkAllowed == difficulty.QuestionMarkAllowed &&
               WrapAround == difficulty.WrapAround &&
               SubtractFlags == difficulty.SubtractFlags &&
               OnlyShowAtCursor == difficulty.OnlyShowAtCursor &&
               AutomaticDiscovery == difficulty.AutomaticDiscovery;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(Name);
        hash.Add(Width);
        hash.Add(Height);
        hash.Add(Mines);
        hash.Add(Lives);
        hash.Add(Safezone);
        hash.Add(DetectionRadius);
        hash.Add(FlagsAllowed);
        hash.Add(QuestionMarkAllowed);
        hash.Add(WrapAround);
        hash.Add(SubtractFlags);
        hash.Add(OnlyShowAtCursor);
        hash.Add(AutomaticDiscovery);
        return hash.ToHashCode();
    }

    internal Difficulty Clone() => Clone(this.Name);
    internal Difficulty Clone(string name)
    {
        return new Difficulty()
        {
            Name = name,
            Width = this.Width,
            Height = this.Height,
            Mines = this.Mines,
            Lives = this.Lives,
            Safezone = this.Safezone,
            DetectionRadius = this.DetectionRadius,
            FlagsAllowed = this.FlagsAllowed,
            QuestionMarkAllowed = this.QuestionMarkAllowed,
            WrapAround = this.WrapAround,
            SubtractFlags = this.SubtractFlags,
            OnlyShowAtCursor = this.OnlyShowAtCursor,
            AutomaticDiscovery = this.AutomaticDiscovery,
            Highscores = new List<HighscoreRecord>()
        };
    }
}