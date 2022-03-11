using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class BoardState
    {
        // Internals
        private readonly CellData[,] Cells;
        private Point cursor;
        private PlayerState playerState;
        private Difficulty difficulty;

        private DateTime startTime;
        private TimeSpan preTime;
        private bool timePaused = true;
        private int lives;
        internal bool highscore = false;

        // Constructors and cloners
        private BoardState(CellData[,] datas) { Cells = datas; Face = Face.Normal; cursor = Board.Center; }

        public BoardState Clone()
        {
            return new BoardState((CellData[,])Cells.Clone())
            {
                playerState = this.playerState,
                cursor = this.cursor.Clone(),
                difficulty = this.difficulty,
                lives = this.lives,

                startTime = this.startTime,
                preTime = this.preTime,
                timePaused = this.timePaused
            };
        }

        internal static BoardState NewGame(Difficulty diff)
        {
            int width = diff.Width;
            int height = diff.Height;

            CellData[,] datas = new CellData[width, height];

            return new BoardState(datas)
            {
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

        internal TOut ApplyAllCells<TOut>(TOut zero, Func<Point, TOut, TOut> fold)
        {
            TOut res = zero;
            ForAllCells((p2) => { res = fold(p2, res); });
            return res;
        }

        internal TOut ApplySurroundingCells<TOut>(Point p, TOut zero, Func<Point, TOut, TOut> fold, bool wrap)
        {
            TOut res = zero;
            ForAllSurroundingCells(p, (p2) => { res = fold(p2, res); }, wrap);
            return res;
        }

        internal void ForAllSurroundingCells(Point p, Action<Point> act, bool wrap)
        {
            for (int ax = p.X - difficulty.DetectionRadius; ax <= p.X + difficulty.DetectionRadius; ax++)
            {
                for (int ay = p.Y - difficulty.DetectionRadius; ay <= p.Y + difficulty.DetectionRadius; ay++)
                {
                    Point loc = new(ax, ay);
                    if (wrap || !CellOutsideBounds(loc)) act(Wrap(loc));
                }
            }
        }

        internal void ForAllCells(Action<Point> act)
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    act(new(x, y));
                }
            }
        }

        private T CheckCell<T>(Point cl, Func<CellData, T> check, T dflt)
        {
            if (CellOutsideBounds(cl))
            {
                if (difficulty.WrapAround)
                    check(Cell(Wrap(cl)));
                return dflt;
            }
            return check(Cell(cl));
        }

        private CellData Cell(Point cl) => Cells[cl.X, cl.Y];

        private int CountSurroundingCells(Point cl, Func<CellData, bool> callback, bool outsideAllowed) => CountSurroundingCells(cl, (cl2) => CheckCell(cl2, callback, false), outsideAllowed);
        private int CountSurroundingCells(Point cl, Func<Point, bool> callback, bool outsideAllowed) => ApplySurroundingCells(cl, 0, (loc, sum) => sum + (callback(loc) ? 1 : 0), outsideAllowed);

        // Oneliner Board Properties
        public PlayerState PlayerState => playerState; 
        internal Difficulty Difficulty => difficulty; 
        public Point Cursor => cursor.Clone(); 
        public TimeSpan Time => (timePaused ? (preTime) : (preTime + (DateTime.Now - startTime))); 

        public bool Paused => timePaused; 

        public Face Face { get; set; }

        public int BoardWidth => Cells.GetLength(0); 
        public int BoardHeight => Cells.GetLength(1); 
        public int Mines => CountCells(x => x.Mine); 
        public int GameMines => difficulty.Mines; 
        public int Flags => CountCells(x => x.Flagged == FlagMarking.Flagged); 
        public int Discovered => CountCells(x => x.Discovered); 
        public int MinesLeft => GameMines - Flags - LivesLost; 
        public int LivesLost => difficulty.Lives - lives; 
        public int Tiles => BoardHeight * BoardWidth; 

        public double DiscoveryRate => (double)Discovered / Tiles; 
        public double MineRate => (double)(LivesLost + Flags) / Mines; 
        internal Rectangle Board => new(0, 0, BoardWidth, BoardHeight);

        // Oneliner Board Queries
        private bool CellOutsideBounds(Point cl) => !Board.Contains(cl);
        public bool CellIsMine(Point cl) => CheckCell(cl, c => c.Mine, false);
        public bool CellIsFlagged(Point cl) => CheckCell(cl, c => c.Flagged == FlagMarking.Flagged, false);
        public bool CellIsDiscovered(Point cl) => CheckCell(cl, c => c.Discovered, false);
        public bool CellIsQuestionMarked(Point cl) => CheckCell(cl, c => c.Flagged == FlagMarking.QuestionMarked, false);
        public int CellMineNumber(Point cl) => CountSurroundingCells(cl, c => c.Mine, difficulty.WrapAround);
        public int CellSubtractedMineNumber(Point cl) => CellMineNumber(cl) - CountSurroundingCells(cl, c => c.Flagged == FlagMarking.Flagged, difficulty.WrapAround);

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

            preTime += (DateTime.Now - startTime);
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

        public int Discover(Point cl)
        {
            List<Point> points = new();
            points.Add(cl);

            int sum = 0;
            bool mineHit = false;

            while (points.Count > 0)
            {
                cl = points[0];
                points.RemoveAt(0);

                // Check discoverable
                if (CellOutsideBounds(cl)) continue;
                if (CellIsDiscovered(cl)) continue;
                if (CellIsFlagged(cl)) continue;

                // Discover
                Cells[cl.X, cl.Y].Discovered = true;
                Cells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;

                // Check for mine
                if (CellIsMine(cl))
                {
                    mineHit = true;
                    break;
                }

                // Continue discovering if encountering an empty cell
                if (CellMineNumber(cl) == 0 && difficulty.AutomaticDiscovery)
                {
                    ForAllSurroundingCells(cl, (p) => points.Add(p), difficulty.WrapAround);
                }
                sum += 1;
            }

            if (mineHit) return -1;
            return sum;
        }

        public int ToggleFlag()
        {
            if (!difficulty.FlagsAllowed) return FailAction();
            if (CellIsDiscovered(cursor)) return FailAction();

            switch (Cell(cursor).Flagged)
            {
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

        public int Flag(Point cl)
        {
            int res = CellIsFlagged(cl) ? 0 : 1;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.Flagged;
            return res;
        }

        public int Unflag(Point cl)
        {
            int res = CellIsFlagged(cl) ? -1 : 0;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.Unflagged;
            return res;
        }

        public int QuestionMark(Point cl)
        {
            int res = CellIsFlagged(cl) ? -1 : 0;

            Cells[cl.X, cl.Y].Flagged = FlagMarking.QuestionMarked;
            return res;
        }

        public Point Wrap(Point cl)
        {
            cl = cl.Clone();
            while (cl.X < 0) cl.X += BoardWidth;
            while (cl.X >= BoardWidth) cl.X -= BoardWidth;

            while (cl.Y < 0) cl.Y += BoardHeight;
            while (cl.Y >= BoardHeight) cl.Y -= BoardHeight;

            return cl;
        }

        public int Distance(Point cl1, Point cl2)
        {
            int xdist = Math.Abs(cl1.X - cl2.X);
            int ydist = Math.Abs(cl1.Y - cl2.Y);

            if (difficulty.WrapAround)
            {
                xdist = Math.Min(xdist, difficulty.Width - xdist);
                ydist = Math.Min(ydist, difficulty.Height - xdist);
            }

            return Math.Max(xdist, ydist);
        }
        public Point MoveCursor(Direction d)
        {
            return d switch
            {
                Direction.Down => SetCursor(new Point(cursor.X, cursor.Y + 1)),
                Direction.Up => SetCursor(new Point(cursor.X, cursor.Y - 1)),
                Direction.Left => SetCursor(new Point(cursor.X - 1, cursor.Y)),
                Direction.Right => SetCursor(new Point(cursor.X + 1, cursor.Y)),
                _ => cursor,
            };
        }

        public Point SetCursor(Point cl)
        {
            cursor = Wrap(cl);
            return cursor;
        }

        public List<Point> CompareForChanges(BoardState other, Rectangle? area = null)
        {
            // Build a list of changed cells
            List<Point> res = new();

            if (area == null)
                Board.ForAll((p) => { if (Cell(p) != other.Cell(p)) res.Add(p); });
            else if (difficulty.SubtractFlags) // overfit because a flag change outside can technically change the view inside the viewport :)
                Board.Intersect(area.Grow(difficulty.DetectionRadius)).ForAll((p) => { if (Cell(p) != other.Cell(p)) res.Add(p); });
            else 
                Board.Intersect(area).ForAll((p) => { if (Cell(p) != other.Cell(p)) res.Add(p); });


            if (difficulty.SubtractFlags)
            {
                List<Point> li = new();
                foreach (Point cl in res)
                {
                    li = ApplySurroundingCells(cl, li, (loc, list) => { if (!list.Contains(loc)) list.Add(loc); return list; }, difficulty.WrapAround);
                }
                res = li;
            }

            res.Add(cursor);
            res.Add(other.cursor);

            return res;
        }

        private void PlaceMines(Point cl)
        {
            int minesLeftToPlace = difficulty.Mines;
            int placementFailures = 0;
            int detectZoneSize = (2 * difficulty.DetectionRadius + 1) * (2 * difficulty.DetectionRadius + 1);
            int maxMines = (int)Math.Floor(0.8 * detectZoneSize);
            int mc;

            Random rng = new();

            // Try to randomly place mines and check if the are valid;
            while (minesLeftToPlace > 0 && placementFailures < 1000)
            {
                placementFailures++;
                int px = rng.Next() % BoardWidth;
                int py = rng.Next() % BoardHeight;

                Point pos = new(px, py);

                if (Distance(cl, pos) <= difficulty.Safezone)
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

    public struct CellData
    {
        public CellData(bool m, bool d, FlagMarking f, bool q) { Mine = m; Discovered = d; Flagged = f; QuestionMarked = q; }
        public bool Mine;
        public bool Discovered;
        public FlagMarking Flagged;
        public bool QuestionMarked;


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

        public override bool Equals(object? obj)
        {
            return obj is CellData data &&
                   Mine == data.Mine &&
                   Discovered == data.Discovered &&
                   Flagged == data.Flagged &&
                   QuestionMarked == data.QuestionMarked;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Mine, Discovered, Flagged, QuestionMarked);
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