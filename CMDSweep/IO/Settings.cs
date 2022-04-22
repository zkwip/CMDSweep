using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.IO;

static class Settings
{
    private static GameSettings _settings = Storage.LoadSettings();

    public static List<Difficulty> DefaultDifficulties => _settings.DefaultDifficulties;

    public static Dictionary<string, ConsoleColor> Colors => _settings.Colors;

    public static Dictionary<string, string> Texts => _settings.Texts;

    public static Dictionary<string, int> Dimensions => _settings.Dimensions;

    public static Dictionary<InputAction, List<ConsoleKey>> Controls => _settings.Controls;

    public static string PlayerName => _settings.PlayerName;

    public static StyleData GetStyle(string handle) => new(Colors[handle + "-fg"], Colors[handle + "-bg"]);

    public static StyleData GetStyle(string fg, string bg) => new(Colors[fg], Colors[bg]);
}
