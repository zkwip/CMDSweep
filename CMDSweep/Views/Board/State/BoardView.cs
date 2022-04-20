using CMDSweep.Geometry;
using CMDSweep.IO;
using System;

namespace CMDSweep.Views.Board.State;

internal class BoardView
{
    private readonly int _scaleX;

    private readonly int _scaleY;

    private readonly int _scrollSafezoneDistance;

    public Rectangle ScrollValidMask { get; private set; } // the area that is still valid (board space)

    public Rectangle RenderMask { get; private set; } // the area the board can be drawn into (screen space)

    public Rectangle Viewport { get; private set; } // rendermask mapped to (board space)

    public Rectangle Board { get; private set; }

    public BoardView(GameSettings settings, Rectangle board)
    {
        _scrollSafezoneDistance = settings.Dimensions["scroll-safezone"];
        _scaleX = settings.Dimensions["cell-size-x"];
        _scaleY = settings.Dimensions["cell-size-y"];

        RenderMask = Rectangle.Zero;
        Viewport = Rectangle.Zero;
        ScrollValidMask = Rectangle.Zero;
        Board = board;
    }

    public BoardView(int scaleX, int scaleY, int scrollSafezoneDistance, Rectangle scrollValidMask, Rectangle renderMask, Rectangle viewport, Rectangle board)
    {
        _scaleX = scaleX;
        _scaleY = scaleY;
        _scrollSafezoneDistance = scrollSafezoneDistance;
        ScrollValidMask = scrollValidMask;
        RenderMask = renderMask;
        Viewport = viewport;
        Board = board;
    }

    private int _offsetX => RenderMask.Left - Viewport.Left * _scaleX;

    private int _offsetY => RenderMask.Top - Viewport.Top * _scaleY;

    public int CellWidth => _scaleX;

    public int CellHeight => _scaleY;

    public Rectangle ScrollSafezone => Viewport.Shrink(_scrollSafezoneDistance);

    public BoardView ScrollTo(Point cursor)
    {
        Offset offset = ScrollSafezone.OffsetOutOfBounds(cursor);
        Rectangle newViewport = Viewport.Shift(offset);
        Rectangle newScrollValidMask = ScrollValidMask.Intersect(newViewport);

        return new(_scaleX, _scaleY, _scrollSafezoneDistance, newScrollValidMask, RenderMask, newViewport, Board);
    }
    public Rectangle MapToRender(Rectangle r) => new(MapToRender(r.TopLeft), MapToRender(r.BottomRight));

    public Point MapToRender(Point p) => new(_offsetX + p.X * _scaleX, _offsetY + p.Y * _scaleY);

    public bool IsVisible(Point p) => Viewport.Contains(p);

    public bool IsScrollValid(Point p) => ScrollValidMask.Contains(p);

    public void ChangeRenderMask(Rectangle newMask)
    {
        RenderMask = newMask;
        ScrollValidMask = Rectangle.Zero;

        Rectangle newVP = new(Viewport.Left, Viewport.Top, RenderMask.Width / _scaleX, RenderMask.Height / _scaleY);

        if (Viewport.Equals(Rectangle.Zero))
            newVP.CenterOn(Board.Center);
        else
            newVP.CenterOn(Viewport.Center);

        Viewport = newVP;
    }

    public Rectangle VisibleBoardSection => Viewport.Intersect(Board);

    public void ValidateViewPort()
    {
        ScrollValidMask = Viewport;
    }

    public void TryCenterViewPort()
    {
        if (Board.Width < ScrollSafezone.Width && Board.Height < ScrollSafezone.Height)
        {
            Viewport.CenterOn(Board.Center);
        }
    }
}
