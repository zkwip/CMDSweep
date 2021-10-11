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
        private Difficulty difficulty;

        private DateTime startTime;
        private TimeSpan preTime;
        private bool timePaused = true;
        private int lives;

        // Constructors and cloners
        private GameState(CellData[,] datas) { Cells = datas; Face = Face.Normal; }

        public GameState Clone() {
            return new GameState((CellData[,])Cells.Clone())
            {
                playerState = this.playerState,
                cursor = this.cursor,
                difficulty = this.difficulty,
                lives = this.lives,

                startTime = this.startTime,
                preTime = this.preTime,
                timePaused = this.timePaused
            };
        }

        public static GameState NewGame(Difficulty diff)
        {
            int width = diff.Width;
            int height = diff.Height;

            CellData[,] datas = new CellData[width, height];
            int x = width / 2;
            int y = height / 2;

            return new GameState(datas)
            {
                cursor = new CellLocation(x, y),
                playerState = PlayerState.NewGame,
                difficulty = diff,

                startTime = DateTime.Now,
                preTime = TimeSpan.Zero,
                timePaused = true,
                lives = diff.Lives,
            };
        }

        // Intermediates and higher level functions
        private int CountCells(Predicate<CellData> p)
        {
            int sum = 0;
            foreach (CellData cell in Cells) if (p(cell)) sum++;
            return sum;
        }

        internal TOut ApplyAllCells<TOut>(TOut zero, Func<CellLocation, TOut, TOut> fold)
        {
            TOut res = zero;
            for (int x = 0; x < BoardWidth; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    CellLocation loc = new CellLocation(x, y);
                    res = fold(loc,res);
                }
            }
            return res;
        }
        
        internal TOut ApplySurroundingCells<TOut>(CellLocation cl, TOut zero, Func<CellLocation, TOut, TOut> fold, bool wrap)
        {
            TOut res = zero;
            for (int ax = cl.X - difficulty.DetectionRadius; ax <= cl.X + difficulty.DetectionRadius; ax++)
            {
                for (int ay = cl.Y - difficulty.DetectionRadius; ay <= cl.Y + difficulty.DetectionRadius; ay++)
                {
                    CellLocation loc = new CellLocation(ax, ay);
                    if (wrap || !CellOutsideBounds(loc))
                        res = fold(Wrap(loc), res);
                }
            }
            return res;
        }

        private T CheckCell<T>(CellLocation cl, Func<CellData,T> check, T dflt)
        {
            if (CellOutsideBounds(cl))
            {
                if (difficulty.WrapAround)
                    check(Cell(Wrap(cl)));
                return dflt;
            }
            return check(Cell(cl));
        }

        private CellData Cell(CellLocation cl) => Cells[cl.X, cl.Y];

        private int CountSurroundingCells(CellLocation cl, Func<CellData, bool> callback, bool outsideAllowed)      => CountSurroundingCells(cl, (cl2) => CheckCell(cl2, callback, false), outsideAllowed);
        private int CountSurroundingCells(CellLocation cl, Func<CellLocation, bool> callback, bool outsideAllowed)  => ApplySurroundingCells(cl, 0, (loc, sum) => sum + (callback(loc)?1:0), outsideAllowed);

        // Oneliner Board Properties
        public PlayerState PlayerState { get => playerState; }
        public Difficulty Difficulty { get => difficulty; }
        public CellLocation Cursor { get => cursor; }
        public TimeSpan Time { get => (timePaused ? (preTime) : (preTime + (DateTime.Now - startTime))); }

        public bool Paused { get => timePaused; }

        public Face Face { get; set; }

        public int BoardWidth { get => Cells.GetLength(0); }
        public int BoardHeight { get => Cells.GetLength(1); }
        public int Mines { get => CountCells(x => x.Mine); }
        public int GameMines { get => difficulty.Mines; }
        public int Flags { get => CountCells(x => x.Flagged == FlagMarking.Flagged); }
        public int Discovered { get => CountCells(x => x.Discovered); }
        public int MinesLeft { get => GameMines - Flags - LivesLost; }
        public int LivesLost { get => difficulty.Lives - lives; }
        public int Tiles { get =>  BoardHeight * BoardWidth; }

        public double DiscoveryRate { get => (double) Discovered / Tiles; }
        public double MineRate { get => (double)(LivesLost + Flags) / Mines; }

        // Oneliner Board Queries
        private bool CellOutsideBounds(CellLocation cl)         => (cl.X < 0 || cl.Y < 0 || cl.X >= BoardWidth || cl.Y >= BoardHeight);
        public bool CellIsMine(CellLocation cl)                 => CheckCell(cl, c => c.Mine, false);
        public bool CellIsFlagged(CellLocation cl)              => CheckCell(cl, c => c.Flagged == FlagMarking.Flagged, false);
        public bool CellIsDiscovered(CellLocation cl)           => CheckCell(cl, c => c.Discovered, false);
        public bool CellIsQuestionMarked(CellLocation cl)       => CheckCell(cl, c => c.Flagged == FlagMarking.QuestionMarked, false);
        public int CellMineNumber(CellLocation cl)              => CountSurroundingCells(cl, c => c.Mine, difficulty.WrapAround);
        public int CellSubtractedMineNumber(CellLocation cl)    => CellMineNumber(cl) - CountSurroundingCells(cl, c => c.Flagged == FlagMarking.Flagged, difficulty.WrapAround);

        //Other queries
        public void Win()
        {
            Console.Title = "You win!";
            playerState = PlayerState.Win;
            Face = Face.Win;
            FreezeGame();
        }

        public void LoseLife()
        {
            FailAction();

            if (--lives > 0) return; 

            Console.Title = "You died!";
            playerState = PlayerState.Dead;
            Face = Face.Dead;
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

        public int FailAction()
        {
            Console.Beep();
            return 0;
        }

        public int Dig()
        {
            // Beep if it's an invalid move 
            if (CellIsDiscovered(cursor)) return 0;
            if (CellIsFlagged(cursor)) return 0;

            Face = Face.Surprise;

            // If needed, start the game
            if (playerState == PlayerState.NewGame) PlaceMines(cursor);

            //Do the digging
            int res = Discover(cursor);

            //Check for death
            if (res < 0) LoseLife();

            if (Discovered + Mines - LivesLost == Tiles) Win();
            
            return res;
        }

        public int Discover(CellLocation cl)
        {
            // Check discoverable
            if (CellOutsideBounds(cl)) return 0;
            if (CellIsDiscovered(cl)) return 0;
            if (CellIsFlagged(cl)) return 0;

            // Discover
            Cells[cl.X, cl.Y].Discovered = true;
            Cells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;

            // Check for mine
            if (CellIsMine(cl)) return -1;
            
            // Continue discovering if encountering an empty cell
            if (CellMineNumber(cl) == 0 && difficulty.AutomaticDiscovery)
                return ApplySurroundingCells(cl, 0, (loc, b) => { int a = Discover(loc); return (a < 0 || b < 0) ? -1 : a + b + 1; }, difficulty.WrapAround);
            return 1;
        }

        public int ToggleFlag()
        {
            if (!difficulty.FlagsAllowed) return FailAction();
            if (CellIsDiscovered(cursor)) return FailAction();

            switch (Cell(cursor).Flagged) {
                case FlagMarking.Flagged:
                    if (difficulty.QuestionMarkAllowed) return QuestionMark(cursor);
                    return Unflag(cursor);

                case FlagMarking.QuestionMarked:
                    return Unflag(cursor);

                case FlagMarking.Unflagged:
                default:
                    return Flag(cursor);
            }
            
        }

        public int Flag(CellLocation cl)
        {
            int res = CellIsFlagged(cl) ? 0 : 1;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.Flagged;
            return res;
        }

        public int Unflag(CellLocation cl)
        {
            int res = CellIsFlagged(cl) ? -1 : 0;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;
            return res;
        }

        public int QuestionMark(CellLocation cl)
        {
            int res = CellIsFlagged(cl) ? -1 : 0;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.QuestionMarked;
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

        public int Distance(CellLocation cl1, CellLocation cl2)
        {
            int xdist = Math.Abs(cl1.X - cl2.X);
            int ydist = Math.Abs(cl1.Y - cl2.Y);

            if (difficulty.WrapAround)
            {
                xdist = Math.Min(xdist, difficulty.Width - xdist);
                ydist = Math.Min(ydist, difficulty.Height - xdist);
            }

            return Math.Max(xdist,ydist);
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

            res = ApplyAllCells(res, (loc, r) => { if (Cell(loc) != other.Cell(loc)) r.Add(loc); return r; });

            if (difficulty.SubtractFlags)
            {
                List<CellLocation> li = new List<CellLocation>();
                foreach(CellLocation cl in res)
                {
                    li = ApplySurroundingCells(cl, li, (loc, list) => { if (!list.Contains(loc)) list.Add(loc); return list; }, difficulty.WrapAround);
                }
                res = li;
            }

            if (this.cursor != other.cursor)
            {
                res.Add(cursor);
                res.Add(other.cursor);
            }
            return res;
        }

        private void PlaceMines(CellLocation cl)
        {
            int minesLeftToPlace = difficulty.Mines;
            int placementFailures = 0;
            int detectZoneSize = (2 * difficulty.DetectionRadius + 1) * (2 * difficulty.DetectionRadius + 1);
            int maxMines = (int)Math.Floor(0.8 * detectZoneSize);
            int mc = 0;

            Random rng = new Random();
            
            // Try to randomly place mines and check if the are valid;
            while (minesLeftToPlace > 0 && placementFailures < 1000)
            {
                placementFailures++;
                int px = rng.Next() % BoardWidth;
                int py = rng.Next() % BoardHeight;

                CellLocation pos = new CellLocation(px, py);

                if (Distance(cl,pos) <= difficulty.Safezone)
                    continue; // No mines at start

                if (CellIsMine(pos))
                    continue; // No duplicate mines
                
                mc = CellMineNumber(pos);
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