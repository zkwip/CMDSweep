using System;

namespace CMDSweep.Rendering;

internal class RenderSheduler<TState>
{
    private TState? _lastState;
    private RefreshMode ModeWaiting;
    private bool IsRendering;
    private readonly IRenderer _renderer;
    private int _counter;
    private int _fullCounter;

    public IChangeableTypeVisualizer<TState> Visualizer { get; }

    public RenderSheduler(IChangeableTypeVisualizer<TState> visualizer, IRenderer renderer)
    {
        Visualizer = visualizer;
        _renderer = renderer;
        ModeWaiting = RefreshMode.None;
        _counter = 0;
        _fullCounter = 0;
    }

    public bool Visualize(TState state, RefreshMode mode)
    {
        // Indicate there is stuff to redraw
        if (mode > ModeWaiting) ModeWaiting = mode;

        // Defer renderer
        if (IsRendering) return false;
        IsRendering = true;

        // Actual render loop
        while (ModeWaiting != RefreshMode.None)
        {
            lock (_renderer)
            {
                _counter++;
                mode = ModeWaiting;
                ModeWaiting = RefreshMode.None;

                if (_lastState == null)
                    mode = RefreshMode.Full;

                try
                {
                    switch (mode)
                    {
                        case RefreshMode.Full:
                            _fullCounter++;
                            _counter = 1;
                            Console.Title = $"Render #{_fullCounter}.{_counter}: full ";
                            Visualizer.Visualize(state);
                            break;

                        default:
                            Console.Title = $"Render #{_fullCounter}.{_counter}: changes";
                            Visualizer.VisualizeChanges(state, _lastState!);
                            break;
                    }
                }
                catch (Exception) { }

                _lastState = state;
            }
        }

        IsRendering = false;
        return true;
    }
}
