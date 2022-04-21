using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Board.State;

internal record class BoardState
{
    // Properties
    public readonly BoardData BoardData;
    public readonly RoundState RoundState;
    public readonly Timing Timing;
    public readonly BoardView View;

    public BoardState(BoardData boardData, RoundState roundStats, Timing timing, BoardView view)
    {
        BoardData = boardData;
        RoundState = roundStats;
        Timing = timing;
        View = view;
    }

    internal static BoardState NewGame(Difficulty diff, GameSettings settings)
    {
        BoardData boardData = BoardData.NewGame(diff);
        RoundState roundData = RoundState.NewGame(diff);

        BoardView view = new BoardView(settings,boardData.Bounds);
        Timing timing = Timing.NewGame(diff);

        return new BoardState(boardData, roundData, timing, view);
    }

    public Difficulty Difficulty => RoundState.Difficulty;

    public int MinesLeft => RoundState.Mines - BoardData.Flags - RoundState.LivesLost;

    public double MineRate => (double)(RoundState.LivesLost + BoardData.Flags) / RoundState.Mines;

    public BoardState Win() => new(BoardData, RoundState.Win(), Timing.Stop(),View);

    public BoardState LoseLife()
    {
        NotifyFailedAction();

        if (RoundState.CanLoseLife)
            return new BoardState(BoardData, RoundState.LoseLife(), Timing, View);

        return new BoardState(BoardData, RoundState.Die(), Timing.Stop(), View);
    }

    public BoardState ResumeGame() => new(BoardData, RoundState, Timing.Resume(), View);

    public BoardState FreezeGame() => new(BoardData, RoundState, Timing.Pause(), View);

    public BoardState NotifyFailedAction()
    {
        Console.Beep();
        return this;
    }

    public BoardState Dig()
    {
        if (BoardData.CellIsDiscovered(BoardData.Cursor)) 
            return NotifyFailedAction();

        if (BoardData.CellIsFlagged(BoardData.Cursor)) 
            return NotifyFailedAction();

        if (RoundState.PlayerState == PlayerState.NewGame) 
            return PlaceMines().Discover(BoardData.Cursor).CheckForWin();

        return Discover(BoardData.Cursor).CheckForWin();
    }

    private BoardState CheckForWin()
    {
        if (BoardData.Discovered + RoundState.Mines - RoundState.LivesLost == BoardData.Tiles) 
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

        return new BoardState(BoardData.Discover(discoveredCells), RoundState, Timing, View);
    }

    public BoardState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardData.CellIsDiscovered(BoardData.Cursor)) return NotifyFailedAction();

        return new(BoardData.ToggleFlag(),RoundState, Timing, View);
    }

    public BoardState MoveCursor(Direction d) => new(BoardData.MoveCursor(d), RoundState, Timing, View);

    public List<Point> CompareForVisibleChanges(BoardState other)
    {
        Rectangle area = View.VisibleBoardSection;
        return BoardData.CompareForVisibleChanges(other.BoardData, area); 
    }

    private BoardState PlaceMines() => new(BoardData.PlaceMines(), RoundState.SetState(PlayerState.Playing), Timing.Resume(), View);

    public BoardState SetPlayerState(PlayerState state) => new(BoardData, RoundState.SetState(state), Timing, View);

    public bool TimeMakesHighscore()
    {
        TimeSpan time = Timing.Time;
        List<HighscoreRecord> scores = Difficulty.Highscores;

        if (scores.Count >= HighscoreTable.highscoreEntries)
        {
            if (time < scores[HighscoreTable.highscoreEntries - 1].Time)
                return true;
            else
                return false;
        }
        return true;
    }

    public bool ScrollIsNeeded => View.ScrollSafezone.Contains(BoardData.Cursor);

    public BoardState Scroll()
    {
        BoardView newView = View.ScrollTo(BoardData.Cursor);

        return new(BoardData, RoundState, Timing, newView);
    }
}
