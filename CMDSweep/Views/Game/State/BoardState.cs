using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game.State;

internal record class BoardState : IRenderState
{
    public readonly CellData[,] Cells;
    public readonly Difficulty Difficulty;
    public readonly Point Cursor;
    public readonly BoardViewState View;
    private readonly int _id;

    public BoardState(CellData[,] cells, Difficulty difficulty, Point cursor, BoardViewState view, int id)
    {
        Cells = cells;
        Difficulty = difficulty;
        Cursor = cursor;
        View = view;
        _id = id;
    }

    public BoardState Scroll()
    {
        BoardViewState newView = View.ScrollTo(Cursor);

        if (newView.ViewPort == View.ViewPort) 
            return this;

        return new(Cells, Difficulty, Cursor, newView, _id + 1);
    }

    public int Id => _id;

    public static BoardState NewGame(Difficulty diff, GameSettings settings, Rectangle renderMask)
    {
        int width = diff.Width;
        int height = diff.Height;

        CellData[,] cells = new CellData[width, height];
        Rectangle board = new(0, 0, width, height);
        Point cursor = board.Center;

        BoardViewState view = BoardViewState.NewGame(settings, board, renderMask);

        return new BoardState(cells, diff, cursor, view, 0);
    }

    private int CountCells(Predicate<CellData> p)
    {
        int sum = 0;
        foreach (CellData cell in Cells) if (p(cell)) sum++;
        return sum;
    }

    internal TOut ApplySurroundingCells<TOut>(Point p, TOut zero, Func<Point, TOut, TOut> fold, bool wrap)
    {
        TOut res = zero;
        ForAllSurroundingCells(p, (p2) => { res = fold(p2, res); }, wrap);
        return res;
    }

    internal void ForAllSurroundingCells(Point p, Action<Point> act, bool wrap)
    {
        for (int ax = p.X - Difficulty.DetectionRadius; ax <= p.X + Difficulty.DetectionRadius; ax++)
        {
            for (int ay = p.Y - Difficulty.DetectionRadius; ay <= p.Y + Difficulty.DetectionRadius; ay++)
            {
                Point loc = new(ax, ay);
                if (wrap || !CellOutsideBounds(loc)) act(Wrap(loc));
            }
        }
    }

    private T CheckCell<T>(Point cl, Func<CellData, T> check, T dflt)
    {
        if (CellOutsideBounds(cl))
        {
            if (Difficulty.WrapAround)
                check(Cell(Wrap(cl)));
            return dflt;
        }
        return check(Cell(cl));
    }

    public CellData Cell(Point cl) => Cells[cl.X, cl.Y];


    public BoardState Flag(Point cl)
    {
        CellData[,] newCells = (CellData[,])Cells.Clone();
        newCells[cl.X, cl.Y].Flagged = FlagMarking.Flagged;
        return new(newCells, Difficulty, Cursor, View, _id + 1);
    }

    public BoardState Unflag(Point cl)
    {
        CellData[,] newCells = (CellData[,])Cells.Clone();
        newCells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;
        return new(newCells, Difficulty, Cursor, View, _id + 1);
    }

    public BoardState QuestionMark(Point cl)
    {
        CellData[,] newCells = (CellData[,])Cells.Clone();
        newCells[cl.X, cl.Y].Flagged = FlagMarking.QuestionMarked;
        return new(newCells, Difficulty, Cursor, View, _id + 1);
    }

    public Point Wrap(Point cl)
    {
        int x = cl.X;
        int y = cl.Y;

        while (x < 0) x += BoardWidth;
        while (x >= BoardWidth) x -= BoardWidth;

        while (y < 0) y += BoardHeight;
        while (y >= BoardHeight) y -= BoardHeight;

        return new Point(x, y);
    }

    public int Distance(Point cl1, Point cl2)
    {
        int xdist = Math.Abs(cl1.X - cl2.X);
        int ydist = Math.Abs(cl1.Y - cl2.Y);

        if (Difficulty.WrapAround)
        {
            xdist = Math.Min(xdist, Difficulty.Width - xdist);
            ydist = Math.Min(ydist, Difficulty.Height - xdist);
        }

        return Math.Max(xdist, ydist);
    }


    private int CountSurroundingCells(Point cl, Func<CellData, bool> callback, bool outsideAllowed) => CountSurroundingCells(cl, (cl2) => CheckCell(cl2, callback, false), outsideAllowed);

    private int CountSurroundingCells(Point cl, Func<Point, bool> callback, bool outsideAllowed) => ApplySurroundingCells(cl, 0, (loc, sum) => sum + (callback(loc) ? 1 : 0), outsideAllowed);

    internal Rectangle Bounds => new(0, 0, BoardWidth, BoardHeight);

    public double DiscoveryRate => (double)Discovered / Tiles;

    public int BoardWidth => Difficulty.Width;

    public int BoardHeight => Difficulty.Height;

    public bool CellOutsideBounds(Point cl) => !Bounds.Contains(cl);

    public bool CellIsMine(Point cl) => CheckCell(cl, c => c.Mine, false);

    public bool CellIsFlagged(Point cl) => CheckCell(cl, c => c.Flagged == FlagMarking.Flagged, false);

    public bool CellIsDiscovered(Point cl) => CheckCell(cl, c => c.Discovered, false);

    public bool CellIsQuestionMarked(Point cl) => CheckCell(cl, c => c.Flagged == FlagMarking.QuestionMarked, false);

    internal List<Point> ExpandChangeHits(List<Point> hits, int detectionRadius, bool wrapAround)
    {
        List<Point> li = new();
        foreach (Point cl in hits)
        {
            li = ApplySurroundingCells(cl, li, (loc, list) => { if (!list.Contains(loc)) list.Add(loc); return list; }, wrapAround);
        }
        return li;
    }

    public BoardState ChangeRenderMask(Rectangle newRenderMask) => new(Cells, Difficulty, Cursor, View.ChangeRenderMask(newRenderMask), _id + 1);

    public int CellMineNumber(Point cl) => CountSurroundingCells(cl, c => c.Mine, Difficulty.WrapAround);

    public int CellSubtractedMineNumber(Point cl) => CellMineNumber(cl) - CountSurroundingCells(cl, c => c.Flagged == FlagMarking.Flagged, Difficulty.WrapAround);

    public int Flags => CountCells(x => x.Flagged == FlagMarking.Flagged);

    public int Discovered => CountCells(x => x.Discovered);

    public int Tiles => BoardHeight * BoardWidth;

    public BoardState Discover(List<Point> discoveredCells)
    {
        CellData[,] newCells = (CellData[,])Cells.Clone();

        foreach (Point cell in discoveredCells)
        {
            newCells[cell.X, cell.Y].Discovered = true;
            newCells[cell.X, cell.Y].Flagged = FlagMarking.Unflagged;
        }

        return new BoardState(newCells, Difficulty, Cursor, View, _id + 1);
    }

    public BoardState MoveCursor(Direction direction)
    {
        Point p = direction switch
        {
            Direction.Down => new(Cursor.X, Cursor.Y + 1),
            Direction.Up => new(Cursor.X, Cursor.Y - 1),
            Direction.Left => new(Cursor.X - 1, Cursor.Y),
            Direction.Right => new(Cursor.X + 1, Cursor.Y),
            _ => Cursor,
        };

        return SetCursor(Wrap(p)).Scroll();

    }

    private BoardState SetCursor(Point cursor) => new(Cells, Difficulty, cursor, View, _id + 1);

    public BoardState ToggleFlag()
    {
        switch (Cell(Cursor).Flagged)
        {
            case FlagMarking.Flagged:
                if (Difficulty.QuestionMarkAllowed)
                    return QuestionMark(Cursor);
                return Unflag(Cursor);

            case FlagMarking.QuestionMarked:
                return Unflag(Cursor);

            case FlagMarking.Unflagged:
            default:
                return Flag(Cursor);
        }
    }

    public List<Point> DiffersFrom(BoardState other, Rectangle area)
    {
        List<Point> hits = new();
        area.ForAll((p) =>
        {
            if (Cell(p) != other.Cell(p)) hits.Add(p);
        });

        return hits;
    }

    public List<Point> CompareForVisibleChanges(BoardState other, Rectangle area)
    {
        if (Difficulty.SubtractFlags)
            area.Grow(Difficulty.DetectionRadius);

        area = area.Intersect(Bounds);

        List<Point> hits = DiffersFrom(other, area);

        if (Difficulty.SubtractFlags) hits = ExpandChangeHits(hits, Difficulty.DetectionRadius, Difficulty.WrapAround);

        if (other.Cursor != Cursor)
        {
            hits.Add(Cursor);
            hits.Add(other.Cursor);
        }

        return hits.FindAll(x=>area.Contains(x));
    }

    public BoardState PlaceMines()
    {
        int minesLeftToPlace = Difficulty.Mines;
        int detectZoneSize = (2 * Difficulty.DetectionRadius + 1) * (2 * Difficulty.DetectionRadius + 1);
        int placementFailures = 0;
        int maxMines = (int)Math.Floor(0.8 * detectZoneSize);
        Random rng = new();

        BoardState state = new(Cells, Difficulty, Cursor, View, _id + 1);

        // Try to randomly place mines and check if the are valid;
        while (minesLeftToPlace > 0 && placementFailures < 1000)
        {
            placementFailures++;
            int px = rng.Next() % state.BoardWidth;
            int py = rng.Next() % state.BoardHeight;

            Point pos = new(px, py);

            if (state.Distance(Cursor, pos) <= state.Difficulty.Safezone)
                continue; // No mines at start

            if (state.CellIsMine(pos))
                continue; // No duplicate mines

            int mc = state.CellMineNumber(pos);
            if (mc > maxMines)
                continue; // Not too many mines around eachoter

            // Succes
            state.Cells[px, py].Mine = true;

            placementFailures = 0;
            minesLeftToPlace--;
        }

        if (minesLeftToPlace > 0)
            throw new Exception("Can't place mine after 1000 random tries");

        return state;
    }

    public List<Point> FindChangedTiles(BoardState other)
    {
        Rectangle area = View.VisibleBoardSection;
        return CompareForVisibleChanges(other, area);
    }
}
