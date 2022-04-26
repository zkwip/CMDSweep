using CMDSweep.Geometry;
using CMDSweep.Data;
using System;

namespace CMDSweep.Views.Game.State;

internal class BoardViewState
{
    private readonly Scale _scale;
    private readonly Offset _offset;
    private readonly int _scrollSafezoneDistance;

    private readonly Rectangle _renderMask;
    private readonly Rectangle _board;

    public static BoardViewState NewGame(GameSettings settings, Rectangle board, Rectangle renderMask)
    {
        int scrollSafezoneDistance = settings.Dimensions["scroll-safezone"];

        int scaleX = settings.Dimensions["cell-size-x"];
        int scaleY = settings.Dimensions["cell-size-y"];

        Scale scale = new(scaleX, scaleY);

        Offset offset = Offset.FromChange(board.Center, renderMask.Center.ScaleBack(scale));
        

        return new BoardViewState(scale, offset, scrollSafezoneDistance, renderMask, board);
    }

    public BoardViewState(Scale scale, Offset offset, int scrollSafezoneDistance, Rectangle renderMask, Rectangle board)
    {
        _scale = scale;
        _scrollSafezoneDistance = scrollSafezoneDistance;
        _renderMask = renderMask;
        _board = board;
        _offset = offset;
    }

    public Rectangle ScrollSafezone => ViewPort.Shrink(_scrollSafezoneDistance);

    public Rectangle ViewPort => _renderMask.ScaleBack(_scale).Shift(_offset.Reverse);

    public Point MapScreenToBoard(Point p) => p.ScaleBack(_scale).Shift(_offset.Reverse);

    public Point MapToRender(Point p) => p.Shift(_offset).Scale(_scale);

    public BoardViewState ScrollTo(Point cursor)
    {
        Offset offset = _offset.Shift(ScrollSafezone.OffsetOutOfBounds(cursor));
        BoardViewState newView = new(_scale, offset, _scrollSafezoneDistance, _renderMask, _board);
        return newView;
    }

    public BoardViewState ChangeRenderMask(Rectangle newMask)
    {
        return new BoardViewState(_scale, _offset, _scrollSafezoneDistance, newMask, _board);
    }

    public Rectangle VisibleBoardSection => ViewPort.Intersect(_board);
}
