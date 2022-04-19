using System;
using System.Collections.Generic;

namespace CMDSweep.IO;

class Difficulty
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
        HashCode hash = new();
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