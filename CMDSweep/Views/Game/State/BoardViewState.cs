using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Game.State;

internal class BoardViewState
{
    private readonly int _scaleX;

    private readonly int _scaleY;

    private readonly int _scrollSafezoneDistance;

    public Rectangle ScrollValidMask { get; private set; } // the area that is still valid (board space)

    public Rectangle RenderMask { get; private set; } // the area the board can be drawn into (screen space)

    public Rectangle Viewport { get; private set; } // rendermask mapped to (board space)

    public Rectangle Board { get; private set; }

    public RenderBufferCopyTask RenderTask { get; private set; }

    public BoardViewState(GameSettings settings, Rectangle board, Rectangle renderMask)
    {
        _scrollSafezoneDistance = settings.Dimensions["scroll-safezone"];
        _scaleX = settings.Dimensions["cell-size-x"];
        _scaleY = settings.Dimensions["cell-size-y"];

        RenderMask = renderMask;
        Board = board;

        Viewport = Rectangle.Centered(Board.Center, new Dimensions(RenderMask.Width / _scaleX, RenderMask.Height / _scaleY));
        ScrollValidMask = Rectangle.Zero;

        RenderTask = RenderBufferCopyTask.None;
    }

    public BoardViewState(int scaleX, int scaleY, int scrollSafezoneDistance, Rectangle scrollValidMask, Rectangle renderMask, Rectangle viewport, Rectangle board)
    {
        _scaleX = scaleX;
        _scaleY = scaleY;
        _scrollSafezoneDistance = scrollSafezoneDistance;
        ScrollValidMask = scrollValidMask;
        RenderMask = renderMask;
        Viewport = viewport;
        Board = board;
        RenderTask = RenderBufferCopyTask.None;
    }

    private int _offsetX => RenderMask.Left - Viewport.Left * _scaleX;

    private int _offsetY => RenderMask.Top - Viewport.Top * _scaleY;

    public int CellWidth => _scaleX;

    public int CellHeight => _scaleY;

    public Rectangle ScrollSafezone => Viewport.Shrink(_scrollSafezoneDistance);

    public BoardViewState ScrollTo(Point cursor)
    {
        
        Offset offset = ScrollSafezone.OffsetOutOfBounds(cursor);
        Rectangle newViewport = Viewport.Shift(offset);
        Rectangle newScrollValidMask = ScrollValidMask.Intersect(newViewport);

        BoardViewState newView = new(_scaleX, _scaleY, _scrollSafezoneDistance, newScrollValidMask, RenderMask, newViewport, Board);

        Rectangle oldCopyArea = this.MapToRender(newView.ScrollValidMask);
        Rectangle newCopyArea = newView.MapToRender(newView.ScrollValidMask);

        newView.RenderTask = new(oldCopyArea, newCopyArea);
        return newView;
    }

    public Rectangle MapToRender(Rectangle r) => new(MapToRender(r.TopLeft), MapToRender(r.BottomRight));

    public Point MapToRender(Point p) => new(_offsetX + p.X * _scaleX, _offsetY + p.Y * _scaleY);

    public bool IsVisible(Point p) => Viewport.Contains(p);

    public bool IsScrollValid(Point p) => ScrollValidMask.Contains(p);

    public void ChangeRenderMask(Rectangle newMask)
    {
        RenderMask = newMask;
        ScrollValidMask = Rectangle.Zero;

        Rectangle newVP = Rectangle.Centered(Viewport.Center, new Dimensions(RenderMask.Width / _scaleX, RenderMask.Height / _scaleY));

        if (Viewport.Equals(Rectangle.Zero))
            Viewport = newVP.CenterOn(Board.Center);
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
            Viewport = Viewport.CenterOn(Board.Center);
        }
    }
}
