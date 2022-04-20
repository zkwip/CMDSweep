using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Data;

#pragma warning disable CS8618
#pragma warning disable CS0649 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
class GameSettings
{
    public List<Difficulty> DefaultDifficulties;
    public Dictionary<string, ConsoleColor> Colors;
    public Dictionary<string, string> Texts;
    public Dictionary<string, int> Dimensions;
    public Dictionary<InputAction, List<ConsoleKey>> Controls;
    public string PlayerName;

    public StyleData GetStyle(string handle) => new(Colors[handle + "-fg"], Colors[handle + "-bg"]);
    public StyleData GetStyle(string fg, string bg) => new(Colors[fg], Colors[bg]);
}
#pragma warning restore CS0649
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
