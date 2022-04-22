namespace CMDSweep.Rendering;

internal class RenderSheduler<TState>
{
    private TState? _lastState;
    private RefreshMode ModeWaiting;
    private bool IsRendering;
    private readonly IRenderer _renderer;

    public IChangeableTypeVisualizer<TState> Visualizer { get; }

    public RenderSheduler(IChangeableTypeVisualizer<TState> visualizer, IRenderer renderer)
    {
        Visualizer = visualizer;
        _renderer = renderer;
        ModeWaiting = RefreshMode.None;
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
                // Reset queue
                mode = ModeWaiting;
                ModeWaiting = RefreshMode.None;

                // Decide what to render.
                if (_lastState == null)
                    mode = RefreshMode.Full;

                //Render
                if (mode == RefreshMode.Full)
                    Visualizer.Visualize(state);
                else
                    Visualizer.VisualizeChanges(state, _lastState!);

                _lastState = state;
            }
        }

        IsRendering = false;
        return true;
    }
}
