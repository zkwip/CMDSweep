using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using CMDSweep.Views.Game.State;

namespace CMDSweep.Views.Game;

internal class BoardBorderVisualizer : ITypeVisualizer<Point>
{
    GameSettings _settings;
    BoardState _boardData;
    BoardViewState _boardView;
    IRenderer _renderer;
    StyleData _borderStyle;

    public BoardBorderVisualizer(GameSettings settings, GameState state, IRenderer renderer, BoardViewState boardView)
    {
        _settings = settings;
        _boardData = state.BoardState;
        _boardView = boardView;
        _renderer = renderer;
        _borderStyle = _settings.GetStyle("border-fg", "cell-bg-out-of-bounds");
    }

    public void Visualize(Point p, RefreshMode _) => Visualize(p);

    public void Visualize(Point p) => _renderer.PrintAtTile(_boardView.MapToRender(p), BorderVisual(p));

    public StyledText BorderVisual(Point p)
    {

        // Corners
        if (p.Equals(new Point(-1, -1)))
            return new(_settings.Texts["border-corner-tl"], _borderStyle);

        if (p.Equals(new Point(_boardData.BoardWidth, -1)))
            return new(_settings.Texts["border-corner-tr"], _borderStyle);

        if (p.Equals(new Point(-1, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-bl"], _borderStyle);

        if (p.Equals(new Point(_boardData.BoardWidth, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-br"], _borderStyle);

        // Edges
        if (p.Y == -1 || p.Y == _boardData.BoardHeight)
            return new(_settings.Texts["border-horizontal"], _borderStyle);

        if (p.X == -1 || p.X == _boardData.BoardWidth)
            return new(_settings.Texts["border-vertical"], _borderStyle);

        throw new ArgumentOutOfRangeException();
    }
}
