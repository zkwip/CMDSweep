using CMDSweep.IO;
using System;

namespace CMDSweep.Views.Board.State;
internal record struct Timing
{
    public readonly bool Paused;
    public readonly DateTime StartTime;
    public readonly TimeSpan PreTime;

    public Timing(bool paused, DateTime startTime, TimeSpan preTime)
    {
        Paused = paused;
        StartTime = startTime;
        PreTime = preTime;
    }

    public static Timing NewGame(Difficulty diff) => new(false, DateTime.Now, TimeSpan.Zero);

    public TimeSpan Time => Paused ? PreTime : PreTime + (DateTime.Now - StartTime);

    internal Timing Stop()
    {
        throw new NotImplementedException();
    }

    public Timing Resume() => new(false, DateTime.Now, PreTime);

    public Timing Pause() => new(true, StartTime, PreTime + (DateTime.Now - StartTime));
}
