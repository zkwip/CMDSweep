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
        private bool QuestionMarkEnabled = true;
        private DateTime startTime;
        private TimeSpan preTime;
        private bool timePaused = true;

        // Constructors and cloners
        private GameState(CellData[,] datas) { Cells = datas;  }

        public GameState Clone() {
            GameState gs =  new GameState((CellData[,])Cells.Clone());

            gs.playerState = playerState;
            gs.cursor = cursor;
            gs.mineCount = mineCount;
            gs.safeZone = safeZone;
            gs.mineDetectRadius = mineDetectRadius;
            gs.QuestionMarkEnabled = QuestionMarkEnabled;
            gs.startTime = startTime;
            gs.preTime = preTime;
            gs.timePaused = timePaused;

            return gs;
        }

        public static GameState NewGame(int width, int height, int mines, int safezone, int radius, bool questionMarkEnabled)
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
            gs.QuestionMarkEnabled = questionMarkEnabled;
            gs.startTime = DateTime.Now;
            gs.preTime = TimeSpan.Zero;
            gs.timePaused = true;

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
        public int CountMines { get => CountCells(x => x.Mine); }
        public int GameMines { get => mineCount; }
        public int CountFlags { get => CountCells(x => x.Flagged == FlagMarking.Flagged); }
        public int CountDiscovered { get => CountCells(x => x.Discovered); }
        public int MinesLeft { get => GameMines - CountFlags; }
        public CellLocation Cursor { get => cursor; }
        public PlayerState PlayerState { get => playerState; }
        public TimeSpan Time { get => (timePaused ? (preTime) : (preTime + (DateTime.Now - startTime))); }
        public bool Paused { get => timePaused; }
        public int Tiles { get =>  BoardHeight * BoardWidth; }
        public double Discovery { get => (double)CountDiscovered / Tiles; }

        // Oneliner Board Queries
        private bool CellOutsideBounds(int x, int y)        => (x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight);
        public bool CellIsMine(int x, int y)                => CheckCell(x, y, c => c.Mine, false);
        public bool CellIsFlagged(int x, int y)             => CheckCell(x, y, c => c.Flagged == FlagMarking.Flagged, false);
        public bool CellIsDiscovered(int x, int y)          => CheckCell(x, y, c => c.Discovered, false);
        public bool CellIsQuestionMarked(int x, int y)      => CheckCell(x, y, c => c.Flagged == FlagMarking.QuestionMarked, false);
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
            if (res < 0) Die();

            if (CountDiscovered + CountMines == Tiles) Win();
            
            return res;
        }

        public void Win()
        {
            Console.Title = "You win!";
            playerState = PlayerState.Win;
            FreezeGame();
        }

        public void Die()
        {
            Console.Title = "You died!";
            playerState = PlayerState.Dead;
            FreezeGame();
        }

        public void ResumeGame()
        {
            timePaused = false;
            startTime = DateTime.Now;
        }

        public void FreezeGame()
        {
            timePaused = true;
            preTime = preTime + (DateTime.Now - startTime);
        }

        public int Discover(int x, int y)
        {
            // Check discoverable
            if (CellOutsideBounds(x, y)) return 0;
            if (CellIsFlagged(x, y)) return 0;
            if (CellIsDiscovered(x, y)) return 0;

            // Discover
            Cells[x, y].Discovered = true;
            Cells[x, y].Flagged = FlagMarking.Unflagged;

            // Check for mine
            if (CellIsMine(x, y)) return -1;

            // Continue discovering if encountering an empty cell
            if (CellMineNumber(x, y) == 0)
                return ApplySurroundingCells(x, y, 0, Discover, (a, b) => (a < 0 || b < 0) ? -1 : a + b);
            return 1;
        }

        public int ToggleFlag()
        {
            int x = cursor.X;
            int y = cursor.Y;

            switch (Cells[x,y].Flagged) {
                case FlagMarking.Flagged:
                    if (QuestionMarkEnabled) return QuestionMark(x, y);
                    return Unflag(x, y);

                case FlagMarking.QuestionMarked:
                    return Unflag(x, y);

                case FlagMarking.Unflagged:
                default:
                    return Flag(x, y);
            }
            
        }
        public int Flag(int x, int y)
        {
            int res = CellIsFlagged(x, y) ? 0 : 1;

            Cells[x, y].Flagged = FlagMarking.Flagged;
            return res;
        }

        public int Unflag(int x, int y)
        {
            int res = CellIsFlagged(x, y) ? -1 : 0;

            Cells[x, y].Flagged = FlagMarking.Unflagged;
            return res;
        }

        public int QuestionMark(int x, int y)
        {
            int res = CellIsFlagged(x, y) ? -1 : 0;

            Cells[x, y].Flagged = FlagMarking.QuestionMarked;
            return res;
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
                
                mc = CellMineNumber(px, py);
                if (mc > maxMines)
                    continue; // Not too many mines around eachoter

                // Succes
                Cells[px, py].Mine = true;

                placementFailures = 0;
                minesLeftToPlace--;
            }

            if (minesLeftToPlace > 0) throw new Exception("Can't place mine after 1000 random tries");

            playerState = PlayerState.Playing;
            ResumeGame();
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
        public CellData(bool m, bool d, FlagMarking f, bool q) { Mine = m; Discovered = d; Flagged = f; QuestionMarked = q; }
        public bool Mine;
        public bool Discovered;
        public FlagMarking Flagged;
        public bool QuestionMarked;

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
        Win,
    }

    public enum FlagMarking
    {
        Unflagged,
        Flagged,
        QuestionMarked,
    }
}