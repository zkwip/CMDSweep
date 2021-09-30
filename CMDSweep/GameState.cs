using System;
using System.Collections.Generic;

namespace CMDSweep
{
    class GameState
    {
        CellData[,] Cells;

        public GameState(CellData[,] datas) { Cells = datas; }
        public GameState Clone() { return new GameState((CellData[,])Cells.Clone()); }

        int BoardWidth { get => Cells.GetLength(0); }
        int BoardHeight { get => Cells.GetLength(1); }

        int Mines { get => CountCells(x => x.Mine); }
        int Flags { get => CountCells(x => x.Flagged); }
        int Discovered { get => CountCells(x => x.Discovered); }
        int MinesLeft { get => Mines - Flags; }


        private int CountCells(Predicate<CellData> p)
        {
            int sum = 0;
            foreach (CellData cell in Cells) if (p(cell)) sum++;
            return sum;
        }

        private TOut ApplySurroundingCells<TOut,Inter>(int x, int y, TOut zero, Func<int, int, Inter> callback, Func<TOut,Inter,TOut> fold)
        {
            TOut res = zero;
            for (int ax = x - 1; ax <= x + 1; ax++)
            {
                for (int ay = y - 1; ay <= y + 1; ay++)
                {
                    res = fold(res,callback(ax, ay));
                }
            }
            return res;
        }

        private T CheckCell<T>(int x, int y, Func<CellData,T> check, T dflt)
        {
            if (CellOutsideBounds(x, y)) return dflt;
            return check(Cells[x, y]);
        }

        private int CountSurroundingCells(int x, int y, Func<CellData, bool> callback) => CountSurroundingCells(x, y, (cx, cy) => CheckCell(cx, cy, callback, false));
        private int CountSurroundingCells(int x, int y, Func<int, int, bool> callback) => ApplySurroundingCells(x, y, 0, callback, (res, boo) => boo ? res : res++);

        // Board queries
        private bool CellOutsideBounds(int x, int y) => (x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight);
        public bool CellIsMine(int x, int y) => CheckCell(x, y, c => c.Mine, false);
        public bool CellIsFlagged(int x, int y) => CheckCell(x, y, c => c.Flagged, false);
        public bool CellIsDiscovered(int x, int y) => CheckCell(x, y, c => c.Discovered, false);
        public int CellMineNumber(int x, int y) => CountSurroundingCells(x, y, c => c.Mine);

        public int Discover(int x, int y)
        {
            // Check discoverable
            if (CellOutsideBounds(x, y)) return 0;
            if (CellIsFlagged(x, y)) return 0;
            if (CellIsMine(x, y)) return -1;
            if (CellIsDiscovered(x, y)) return 0;

            // Discover
            Cells[x, y].Discovered = true;

            // Continue discovering if encountering an empty cell
            if (CellMineNumber(x, y) == 0) return ApplySurroundingCells(x, y, 0, Discover, (a, b) => (a < 0 || b < 0) ? -1 : a + b);
            return 1;
        }

        public List<CellLocation> CompareForChanges(GameState other)
        {
            List<CellLocation> res = new List<CellLocation>();
            for (int x = 0; x < BoardWidth; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    if (Cells[x, y] != other.Cells[x, y]) res.Add(new CellLocation(x, y));
                }
            }
            return res;
        }
    }

    struct CellLocation
    {
        public CellLocation(int x, int y) { X = x; Y = y; }

        public readonly int X;
        public readonly int Y;
    }

    struct CellData
    {
        public CellData(bool m, bool d, bool f) { Mine = m; Discovered = d; Flagged = f; }
        public readonly bool Mine;
        public bool Discovered;
        public bool Flagged;

        public override bool Equals(object obj)
        {
            return obj is CellData && Equals((CellData)obj);
        }

        public static bool operator ==(CellData c1, CellData c2)
        {
            return c1.Mine == c2.Mine &&
                   c1.Discovered == c2.Discovered &&
                   c1.Flagged == c2.Flagged;
        }

        public static bool operator !=(CellData c1, CellData c2)
        {
            return c1.Mine != c2.Mine ||
                   c1.Discovered != c2.Discovered ||
                   c1.Flagged != c2.Flagged;
        }
    }
}
