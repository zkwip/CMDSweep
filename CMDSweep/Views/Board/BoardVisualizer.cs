using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

partial class BoardVisualizer : IChangeableTypeVisualizer<BoardState>
{
    private readonly IRenderer _renderer;
    private BoardPopupVisualizer _boardPopups;
    private StatboardVisualizer _statboardVisualizer;
    private TileVisualizer _tileVisualizer;
    private StyleData _hideStyle;

    public BoardVisualizer(IRenderer renderer, GameSettings settings, BoardState currentState)
    {
        _renderer = renderer;
        _hideStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        _statboardVisualizer = new StatboardVisualizer(_renderer, settings);
        _boardPopups = new BoardPopupVisualizer(_renderer, settings);
        _tileVisualizer = new TileVisualizer(_renderer, settings, currentState);
    }

    public void Visualize(BoardState state)
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

    public void VisualizeChanges(BoardState state, BoardState previousState)
    {
        List<Point> changes;
        changes = state.CompareForVisibleChanges(previousState);

        foreach (Point p in changes) _tileVisualizer.Visualize(p);

        _statboardVisualizer.Visualize(state);
    }
}
