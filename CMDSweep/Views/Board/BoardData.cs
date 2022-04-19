using CMDSweep.Geometry;
using CMDSweep.IO;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

internal record class BoardData
{
    internal readonly CellData[,] Cells;
    internal readonly Difficulty Difficulty;

    public BoardData(CellData[,] cells, Difficulty difficulty)
    {
        Cells = cells;
        Difficulty = difficulty;
    }

    public static BoardData NewGame(Difficulty diff)
    {
        int width = diff.Width;
        int height = diff.Height;

        CellData[,] cells = new CellData[width, height];
        return new BoardData(cells, diff);
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


    public BoardData Flag(Point cl)
    {
        CellData[,] newCells = (CellData[,])(Cells.Clone());
        newCells[cl.X, cl.Y].Flagged = FlagMarking.Flagged;
        return new(newCells, Difficulty);
    }

    public BoardData Unflag(Point cl)
    {
        CellData[,] newCells = (CellData[,])(Cells.Clone());
        newCells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;
        return new(newCells, Difficulty);
    }

    public BoardData QuestionMark(Point cl)
    {
        CellData[,] newCells = (CellData[,])(Cells.Clone());
        newCells[cl.X, cl.Y].Flagged = FlagMarking.QuestionMarked;
        return new(newCells, Difficulty);
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

    public int CellMineNumber(Point cl) => CountSurroundingCells(cl, c => c.Mine, Difficulty.WrapAround);

    public int CellSubtractedMineNumber(Point cl) => CellMineNumber(cl) - CountSurroundingCells(cl, c => c.Flagged == FlagMarking.Flagged, Difficulty.WrapAround);

    public int Flags => CountCells(x => x.Flagged == FlagMarking.Flagged);

    public int Discovered => CountCells(x => x.Discovered);

    public int Tiles => BoardHeight * BoardWidth;

    public BoardData Discover(List<Point> discoveredCells)
    {
        CellData[,] newCells = (CellData[,])Cells.Clone();

        foreach (Point cell in discoveredCells)
        {
            Cells[cell.X, cell.Y].Discovered = true;
            Cells[cell.X, cell.Y].Flagged = FlagMarking.Unflagged;
        }

        return new BoardData(newCells, Difficulty);
    }

    public BoardData ToggleFlag(Point cursor)
    {
        switch (Cell(cursor).Flagged)
        {
            case FlagMarking.Flagged:
                if (Difficulty.QuestionMarkAllowed)
                    return QuestionMark(cursor);
                return Unflag(cursor);

            case FlagMarking.QuestionMarked:
                return Unflag(cursor);

            case FlagMarking.Unflagged:
            default:
                return Flag(cursor);
        }
    }

    public List<Point> DiffersFrom(BoardData other, Rectangle area)
    {
        List<Point> hits = new List<Point>();
        area.ForAll((Point p) =>
        {
            if (Cell(p) != other.Cell(p)) hits.Add(p);
        });

        return hits;
    }

    public BoardData PlaceMines(Point seedPoint)
    {
        int minesLeftToPlace = Difficulty.Mines;
        int detectZoneSize = (2 * Difficulty.DetectionRadius + 1) * (2 * Difficulty.DetectionRadius + 1);
        int placementFailures = 0;
        int maxMines = (int)Math.Floor(0.8 * detectZoneSize);
        Random rng = new();

        BoardData bd = new(Cells, Difficulty);

        // Try to randomly place mines and check if the are valid;
        while (minesLeftToPlace > 0 && placementFailures < 1000)
        {
            placementFailures++;
            int px = rng.Next() % BoardWidth;
            int py = rng.Next() % BoardHeight;

            Point pos = new(px, py);

            if (bd.Distance(seedPoint, pos) <= Difficulty.Safezone)
                continue; // No mines at start

            if (bd.CellIsMine(pos))
                continue; // No duplicate mines

            int mc = bd.CellMineNumber(pos);
            if (mc > maxMines)
                continue; // Not too many mines around eachoter

            // Succes
            bd.Cells[px, py].Mine = true;

            placementFailures = 0;
            minesLeftToPlace--;
        }

        if (minesLeftToPlace > 0) throw new Exception("Can't place mine after 1000 random tries");

        return bd;
    }
}
