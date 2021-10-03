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
        public GameState(CellData[,] datas) { Cells = datas;  }

        public GameState Clone() {
            GameState gs =  new GameState((CellData[,])Cells.Clone());

            gs.playerState = playerState;
            gs.cursor = cursor;
            gs.mineCount = mineCount;
            gs.safeZone = safeZone;
            gs.mineDetectRadius = mineDetectRadius;

            return gs;
        }

        public static GameState NewGame(int width, int height, int mines, int safezone, int radius)
        {
            CellData[,] datas = new CellData[width, height];
            int x = width / 2;
            int y = height / 2;

            GameState gs = new GameState(datas);

            gs.cursor = new CellLocation(x, y);
            gs.playerState = PlayerState.NewGame;
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
        private int CountSurroundingCells(int x, int y, Func<int, int, bool> callback)  => ApplySurroundingCells(x, y, 0, callback, (sum, val) => sum + (val?1:0));

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
            if (playerState == PlayerState.NewGame) PlaceMines(x, y);

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
            if (CellIsDiscovered(x, y)) return 0;

            // Discover
            Cells[x, y].Discovered = true;

            // Check for mine
            if (CellIsMine(x, y)) return -1;

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
            if (CellIsDiscovered(x, y)) return 0;

            Cells[x, y].Flagged = true;
            return 1;
        }

        public int Unflag(int x, int y)
        {
            if (!CellIsFlagged(x, y)) return 0;

            Cells[x, y].Flagged = false;
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

        private void PlaceMines(int x, int y)
        {
            int minesLeftToPlace = mineCount;
            int placementFailures = 0;
            int detectZoneSize = (2 * mineDetectRadius + 1) * (2 * mineDetectRadius + 1);
            int maxMines = (int)Math.Floor(0.8 * detectZoneSize);
            int mc = 0;

            Random rng = new Random();
            
            // Try to randomly place mines and check if the are valid;
            while (minesLeftToPlace > 0 && placementFailures < 1000)
            {
                placementFailures++;
                int px = rng.Next() % BoardWidth;
                int py = rng.Next() % BoardHeight;

                if ((px <= x + safeZone) && (py <= y + safeZone) && (px >= x - safeZone) && (py >= y - safeZone))
                    continue; // No mines at start

                if (CellIsMine(px, py))
                    continue; // No duplicate mines

                Console.Title = "test3";
                mc = CellMineNumber(px, py);
                if (mc > maxMines)
                    continue; // Not too many mines around eachoter

                // Succes
                Console.Title = "test4";
                Cells[px, py].Mine = true;

                placementFailures = 0;
                minesLeftToPlace--;
            }

            if (minesLeftToPlace > 0) throw new Exception("Can't place mine after 1000 random tries");

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