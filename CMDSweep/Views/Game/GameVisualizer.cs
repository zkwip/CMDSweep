using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;
using System.Collections.Generic;

namespace CMDSweep.Views.Game;

partial class GameVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly IRenderer _renderer;
    private BoardPopupVisualizer _boardPopups;
    private StatboardVisualizer _statboardVisualizer;
    private TileVisualizer _tileVisualizer;
    private StyleData _hideStyle;

    public GameVisualizer(IRenderer renderer, GameSettings settings, GameState initialState)
    {
        _renderer = renderer;
        _hideStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        _statboardVisualizer = new StatboardVisualizer(_renderer, settings);
        _boardPopups = new BoardPopupVisualizer(_renderer, settings);
        _tileVisualizer = new TileVisualizer(_renderer, settings, initialState);
    }

    public void Visualize(GameState state)
    {
        state.View.TryCenterViewPort();

        // Border
        _renderer.ClearScreen(_hideStyle);
        state.View.VisibleBoardSection.ForAll(p => _tileVisualizer.Visualize(p));

        // Extras
        _boardPopups.Visualize(state);
        _statboardVisualizer.Visualize(state);

        _renderer.HideCursor(_hideStyle);
        state.View.ValidateViewPort();
    }

    public void VisualizeChanges(GameState state, GameState previousState)
    {
        List<Point> changes;
        changes = state.CompareForVisibleChanges(previousState);

        foreach (Point p in changes) _tileVisualizer.Visualize(p);

        _statboardVisualizer.Visualize(state);
    }
}
