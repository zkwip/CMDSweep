using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class GameState
    {
        // Internals
        private CellData[,] Cells;
        private CellLocation cursor;
        private PlayerState playerState;
        private int mineCount = 0;
        private int safeZone = 0;
        private int mineDetectRadius = 1;

        // Constructors and cloners
        public GameState(CellData[,] datas,CellLocation c, PlayerState ps) { Cells = datas; cursor = c; playerState = ps; }
        public GameState Clone() { return new GameState((CellData[,])Cells.Clone(),cursor,playerState); }
        public static GameState NewGame(int width, int height, int mines, int safezone, int radius)
        {
            CellData[,] datas = new CellData[width, height];
            int x = width / 2;
            int y = height / 2;

            GameState gs = new GameState(datas,new CellLocation(x,y),PlayerState.NewGame);
            gs.mineCount = mines;
            gs.safeZone = safezone;
            gs.mineDetectRadius = radius;

            return gs;
        }

        // Intermediates and higher level functions
        private int CountCells(Predicate<CellData> p)
        {
            int sum = 0;
            foreach (CellData cell in Cells) if (p(cell)) sum++;
            return sum;
        }

        private TOut ApplySurroundingCells<TOut,Inter>(int x, int y, TOut zero, Func<int, int, Inter> callback, Func<TOut,Inter,TOut> fold)
        {
            TOut res = zero;
            for (int ax = x - mineDetectRadius; ax <= x + mineDetectRadius; ax++)
            {
                for (int ay = y - mineDetectRadius; ay <= y + mineDetectRadius; ay++)
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

        private int CountSurroundingCells(int x, int y, Func<CellData, bool> callback)  => CountSurroundingCells(x, y, (cx, cy) => CheckCell(cx, cy, callback, false));
        private int CountSurroundingCells(int x, int y, Func<int, int, bool> callback)  => ApplySurroundingCells(x, y, 0, callback, (res, boo) => boo ? res : res++);

        // Oneliner Board Properties
        public int BoardWidth { get => Cells.GetLength(0); }
        public int BoardHeight { get => Cells.GetLength(1); }
        public int Mines { get => CountCells(x => x.Mine); }
        public int Flags { get => CountCells(x => x.Flagged); }
        public int Discovered { get => CountCells(x => x.Discovered); }
        public int MinesLeft { get => Mines - Flags; }
        public CellLocation Cursor { get => cursor; }
        public PlayerState PlayerState { get => playerState; }

        // Oneliner Board Queries
        private bool CellOutsideBounds(int x, int y)        => (x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight);
        public bool CellIsMine(int x, int y)                => CheckCell(x, y, c => c.Mine, false);
        public bool CellIsFlagged(int x, int y)             => CheckCell(x, y, c => c.Flagged, false);
        public bool CellIsDiscovered(int x, int y)          => CheckCell(x, y, c => c.Discovered, false);
        public int CellMineNumber(int x, int y)             => CountSurroundingCells(x, y, c => c.Mine);

        //Other queries
        public int Dig()
        {
            int x = cursor.X;
            int y = cursor.Y;

            // If needed, start the game
            if (playerState == PlayerState.NewGame) PlaceMines(x, y, mineCount, safeZone);

            //Do the digging
            int res = Discover(x,y);

            //Check for death
            if (res < 0) playerState = PlayerState.Dead;

            return res;
        }


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
            if (CellMineNumber(x, y) == 0)
                return ApplySurroundingCells(x, y, 0, Discover, (a, b) => (a < 0 || b < 0) ? -1 : a + b);
            return 1;
        }

        public int ToggleFlag()
        {
            if (CellIsFlagged(cursor.X, cursor.Y))
                return Unflag(cursor.X, cursor.Y);
            else
                return Flag(cursor.X, cursor.Y);
        }
        public int Flag(int x, int y)
        {
            if (CellIsFlagged(x, y) || CellIsDiscovered(x,y)) return 0;
            else Cells[x, y].Flagged = true;
            return 1;
        }

        public int Unflag(int x, int y)
        {
            if (!CellIsFlagged(x, y)) return 0;
            else Cells[x, y].Flagged = false;
            return -1;
        }

        public CellLocation Wrap(CellLocation cl)
        {
            while (cl.X < 0) cl.X += BoardWidth;
            while (cl.X >= BoardWidth) cl.X -= BoardWidth;

            while (cl.Y < 0) cl.Y += BoardHeight;
            while (cl.Y >= BoardHeight) cl.Y -= BoardHeight;

            return cl;
        }
        
        public CellLocation MoveCursor(Direction d)
        {
            switch (d)
            {
                case Direction.Down:
                    return SetCursor(new CellLocation(cursor.X, cursor.Y + 1));
                case Direction.Up:
                    return SetCursor(new CellLocation(cursor.X, cursor.Y - 1));
                case Direction.Left:
                    return SetCursor(new CellLocation(cursor.X - 1, cursor.Y));
                case Direction.Right:
                    return SetCursor(new CellLocation(cursor.X + 1, cursor.Y));
            }
            return cursor;
        }

        public CellLocation SetCursor(CellLocation cl)
        {
            cursor = Wrap(cl);
            return cursor;
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

            if (this.cursor != other.cursor)
            {
                res.Add(cursor);
                res.Add(other.cursor);
            }
            return res;
        }

        private void PlaceMines(int x, int y, int count, int savezone)
        {
            int left = count;
            int fails = 0;
            Random rng = new Random();
            
            // Try to randomly place mines and check if the are valid;
            while (left > 0 && fails < 1000)
            {
                fails++;
                int px = rng.Next() % BoardWidth;
                int py = rng.Next() % BoardHeight;

                if (x <= px + savezone && y <= py + savezone && x >= px - savezone && y >= py - savezone)
                    continue; // No mines at start

                if (CellIsMine(px, py))
                    continue; // No duplicate mines

                if (CellMineNumber(px, py) > 3 * (mineDetectRadius + 1) * mineDetectRadius)
                    continue; // Not too many mines around eachoter

                Cells[px, py].Mine = true;

                fails = 0;
                left--;
            }

            if (left > 0) throw new Exception("Can't place mine after 1000 random tries");
            playerState = PlayerState.Playing;
        }
    }

    public struct CellLocation
    {
        public CellLocation(int x, int y) { X = x; Y = y; }

        public int X;
        public int Y;

        public static bool operator ==(CellLocation c1, CellLocation c2)
        {
            return c1.X == c2.X &&
                   c1.Y == c2.Y;
        }

        public static bool operator !=(CellLocation c1, CellLocation c2)
        {
            return c1.X != c2.X ||
                   c1.Y != c2.Y;
        }
    }

    public struct CellData
    {
        public CellData(bool m, bool d, bool f) { Mine = m; Discovered = d; Flagged = f; }
        public bool Mine;
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

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    public enum PlayerState
    {
        NewGame,
        Playing,
        Dead,
    }
}