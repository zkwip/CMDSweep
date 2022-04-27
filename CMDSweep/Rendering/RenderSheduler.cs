using System;

namespace CMDSweep.Rendering;

internal class RenderSheduler<TState> where TState : IRenderState
{
    private readonly IRenderer _renderer;
    private readonly IChangeableTypeVisualizer<TState> _visualizer;

    private TState? _lastState;
    private TState? _nextState;
    private RefreshMode _modeWaiting;
    private bool _isRendering;

    private int _counter;
    private int _fullCounter;
    private string _message;


    public RenderSheduler(IChangeableTypeVisualizer<TState> visualizer, IRenderer renderer)
    {
        _renderer = renderer;
        _visualizer = visualizer;
        _modeWaiting = RefreshMode.None;

        _counter = 0;
        _fullCounter = 0;
        _message = "Nothing to report.";
    }

    public bool Refresh(TState state, RefreshMode mode)
    {
        if (mode > _modeWaiting) 
            _modeWaiting = mode;

        _nextState = state;

        if (_isRendering) 
            return false;

        _isRendering = true;

        while (_modeWaiting != RefreshMode.None)
        {
            lock (_renderer)
            {
                mode = _modeWaiting;
                _modeWaiting = RefreshMode.None;

                if (_lastState == null)
                    mode = RefreshMode.Full;

                RunVisualization(NextState, mode);
            }
        }

        _isRendering = false;
        return true;
    }

    private TState NextState => _nextState!;

    private void RunVisualization(TState state, RefreshMode mode)
    {
        try
        {
            _counter++;
            _message = $"State id: {state.Id}";

            switch (mode)
            {
                case RefreshMode.Full:
                    _fullCounter++;
                    _counter = 1;
                    Console.Title = $"Render #{_fullCounter}.{_counter}: full - {_message}";
                    _visualizer.Visualize(state);
                    break;

                default:
                    Console.Title = $"Render #{_fullCounter}.{_counter}: changes - {_message}";
                    _visualizer.VisualizeChanges(state, _lastState!);
                    break;
            }

            _lastState = state;
        }
        catch (Exception ex)
        {
            _message = ex.Message;
            Console.Title = $"Render #{_fullCounter}.{_counter}: changes - {_message}";
        }
    }
}
