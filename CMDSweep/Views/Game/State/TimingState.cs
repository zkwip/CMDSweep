using CMDSweep.Rendering;
using System;

namespace CMDSweep.Views.Game.State;
internal record struct TimingState : IRenderState
{
    public readonly bool Paused;
    public readonly DateTime StartTime;
    public readonly TimeSpan PreTime;

    public TimingState(bool paused, DateTime startTime, TimeSpan preTime)
    {
        Paused = paused;
        StartTime = startTime;
        PreTime = preTime;
    }

    public int Id => 0;

    public static TimingState NewGame() => new(true, DateTime.Now, TimeSpan.Zero);

    public TimeSpan Time => Paused ? PreTime : PreTime + (DateTime.Now - StartTime);

    public TimingState Resume() => new(false, DateTime.Now, PreTime);

    public TimingState Pause() => new(true, StartTime, PreTime + (DateTime.Now - StartTime));
}
