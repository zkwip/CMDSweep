using CMDSweep.Geometry;
using CMDSweep.IO;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

internal record class BoardState
{
    // Properties
    internal readonly BoardData BoardData;
    public readonly RoundData RoundData;
    public readonly Timing Timing;

    public BoardState(BoardData boardData, RoundData roundData, Timing timing)
    {
        BoardData = boardData;
        RoundData = roundData;
        Timing = timing;
    }

    internal static BoardState NewGame(Difficulty diff)
    {
        BoardData boardData = BoardData.NewGame(diff);
        RoundData roundData = RoundData.NewGame(diff);
        Timing timing = Timing.NewGame(diff);

        return new BoardState(boardData, roundData, timing);
    }

    public Difficulty Difficulty => RoundData.Difficulty;

    public int MinesLeft => RoundData.Mines - BoardData.Flags - RoundData.LivesLost;

    public double MineRate => (double)(RoundData.LivesLost + BoardData.Flags) / RoundData.Mines;

    public BoardState Win() => new(BoardData, RoundData.Win(), Timing.Stop());

    public BoardState LoseLife()
    {
        NotifyFailedAction();
        if (RoundData.CanLoseLife)
            return new BoardState(BoardData, RoundData.LoseLife(), Timing);
        return new BoardState(BoardData, RoundData.Die(), Timing.Stop());
    }

    public BoardState ResumeGame() => new(BoardData, RoundData, Timing.Resume());

    public BoardState FreezeGame() => new(BoardData, RoundData, Timing.Pause());

    public BoardState NotifyFailedAction()
    {
        Console.Beep();
        return this;
    }

    public BoardState Dig()
    {
        if (BoardData.CellIsDiscovered(RoundData.Cursor)) 
            return NotifyFailedAction();

        if (BoardData.CellIsFlagged(RoundData.Cursor)) 
            return NotifyFailedAction();

        if (RoundData.PlayerState == PlayerState.NewGame) 
            return PlaceMines().Discover(RoundData.Cursor).CheckForWin();

        return Discover(RoundData.Cursor).CheckForWin();
    }

    private BoardState CheckForWin()
    {
        if (BoardData.Discovered + RoundData.Mines - RoundData.LivesLost == BoardData.Tiles) 
            return Win();
        return this;
    }

    public BoardState Discover(Point cl)
    {
        List<Point> points = new();
        List<Point> discoveredCells = new();
        points.Add(cl);

        int sum = 0;
        bool mineHit = false;

        while (points.Count > 0)
        {
            cl = points[0];
            points.RemoveAt(0);

            // Check discoverable
            if (BoardData.CellOutsideBounds(cl)) continue;
            if (BoardData.CellIsDiscovered(cl)) continue;
            if (BoardData.CellIsFlagged(cl)) continue;

            discoveredCells.Add(cl);

            // Check for mine
            if (BoardData.CellIsMine(cl))
            {
                mineHit = true;
                break;
            }

            // Continue discovering if encountering an empty cell
            if (BoardData.CellMineNumber(cl) == 0 && Difficulty.AutomaticDiscovery)
            {
                BoardData.ForAllSurroundingCells(cl, (p) => points.Add(p), Difficulty.WrapAround);
            }
            sum += 1;
        }

        if (mineHit) return LoseLife();

        return new BoardState(BoardData.Discover(discoveredCells), RoundData, Timing);
    }

    public BoardState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardData.CellIsDiscovered(RoundData.Cursor)) return NotifyFailedAction();

        return new(BoardData.ToggleFlag(RoundData.Cursor),RoundData, Timing);

    }
    public BoardState MoveCursor(Direction d) => new(BoardData, RoundData.MoveCursor(d), Timing);

    public List<Point> CompareForChanges(BoardState other, Rectangle? rect = null)
    {
        List<Point> hits = new();
        if (rect == null) rect = BoardData.Bounds;
        Rectangle area = (Rectangle)rect;

        if (Difficulty.SubtractFlags)
            area.Grow(Difficulty.DetectionRadius);

        area = area.Intersect(BoardData.Bounds);

        if (Difficulty.SubtractFlags) hits = BoardData.ExpandChangeHits(hits, Difficulty.DetectionRadius, Difficulty.WrapAround);

        if (other.RoundData.Cursor != RoundData.Cursor)
        {
            hits.Add(RoundData.Cursor);
            hits.Add(other.RoundData.Cursor);
        }

        return hits;
    }

    private BoardState PlaceMines() => new(BoardData.PlaceMines(RoundData.Cursor), RoundData.SetState(PlayerState.Playing), Timing.Resume());

    public BoardState SetPlayerState(PlayerState state) => new(BoardData, RoundData.SetState(state), Timing);

    public bool TimeMakesHighscore()
    {
        TimeSpan time = Timing.Time;
        List<HighscoreRecord> scores = Difficulty.Highscores;

        if (scores.Count >= Highscores.highscoreEntries)
        {
            if (time < scores[Highscores.highscoreEntries - 1].Time)
                return true;
            else
                return false;
        }
        return true;
    }
}

enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

enum PlayerState
{
    NewGame,
    Playing,
    Dead,
    Win,
    EnteringHighscore,
    ShowingHighscores,
}

enum FlagMarking
{
    Unflagged,
    Flagged,
    QuestionMarked,
}
