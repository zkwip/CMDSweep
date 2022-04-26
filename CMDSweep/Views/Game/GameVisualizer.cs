using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;

namespace CMDSweep.Views.Game;

partial class GameVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly IRenderer _renderer;

    private GamePopupVisualizer _gamePopupVisualizer;
    private StatBarVisualizer _statBarVisualizer;
    private BoardVisualizer _boardVisualizer;

    private StyleData _hideStyle;

    public GameVisualizer(IRenderer renderer, GameSettings settings, GameState initialState)
    {
        _renderer = renderer;
        _hideStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        _statBarVisualizer = new StatBarVisualizer(_renderer, settings);
        _gamePopupVisualizer = new GamePopupVisualizer(_renderer, settings);
        _boardVisualizer = new BoardVisualizer(_renderer, settings, initialState.BoardState);
    }

    public void Visualize(GameState state)
    {
        _renderer.ClearScreen(_hideStyle);
        _boardVisualizer.Visualize(state.BoardState);
        // Extras
        _gamePopupVisualizer.Visualize(state);
        _statBarVisualizer.Visualize(state);

        _renderer.HideCursor(_hideStyle);
    }

    public void VisualizeChanges(GameState state, GameState previousState)
    {
        _boardVisualizer.VisualizeChanges(state.BoardState, previousState.BoardState);
        _statBarVisualizer.Visualize(state);
    }
}
