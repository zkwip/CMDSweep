using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game.State;

internal record class GameState
{
    // Properties
    public readonly BoardState BoardState;
    public readonly BoardViewState View;
    public readonly GameProgressState ProgressState;
    public readonly TimingState Timing;

    public GameState(BoardState boardData, GameProgressState roundStats, TimingState timing, BoardViewState view)
    {
        BoardState = boardData;
        ProgressState = roundStats;
        Timing = timing;
        View = view;
    }

    internal static GameState NewGame(Difficulty diff, GameSettings settings)
    {
        BoardState boardData = BoardState.NewGame(diff);
        GameProgressState roundData = GameProgressState.NewGame(diff);

        BoardViewState view = new BoardViewState(settings,boardData.Bounds);
        TimingState timing = TimingState.NewGame(diff);

        return new GameState(boardData, roundData, timing, view);
    }

    public Difficulty Difficulty => ProgressState.Difficulty;

    public int MinesLeft => ProgressState.Mines - BoardState.Flags - ProgressState.LivesLost;

    public double MineRate => (double)(ProgressState.LivesLost + BoardState.Flags) / ProgressState.Mines;

    public GameState Win() => new(BoardState, ProgressState.Win(), Timing.Stop(),View);

    public GameState LoseLife()
    {
        NotifyFailedAction();

        if (ProgressState.CanLoseLife)
            return new GameState(BoardState, ProgressState.LoseLife(), Timing, View);

        return new GameState(BoardState, ProgressState.Die(), Timing.Stop(), View);
    }

    public GameState ResumeGame() => new(BoardState, ProgressState, Timing.Resume(), View);

    public GameState FreezeGame() => new(BoardState, ProgressState, Timing.Pause(), View);

    public GameState NotifyFailedAction()
    {
        Console.Beep();
        return this;
    }

    public GameState Dig()
    {
        if (BoardState.CellIsDiscovered(BoardState.Cursor)) 
            return NotifyFailedAction();

        if (BoardState.CellIsFlagged(BoardState.Cursor)) 
            return NotifyFailedAction();

        if (ProgressState.PlayerState == PlayerState.NewGame) 
            return PlaceMines().Discover(BoardState.Cursor).CheckForWin();

        return Discover(BoardState.Cursor).CheckForWin();
    }

    private GameState CheckForWin()
    {
        if (BoardState.Discovered + ProgressState.Mines - ProgressState.LivesLost == BoardState.Tiles) 
            return Win();
        return this;
    }

    public GameState Discover(Point cl)
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
            if (BoardState.CellOutsideBounds(cl)) continue;
            if (BoardState.CellIsDiscovered(cl)) continue;
            if (BoardState.CellIsFlagged(cl)) continue;

            discoveredCells.Add(cl);

            // Check for mine
            if (BoardState.CellIsMine(cl))
            {
                mineHit = true;
                break;
            }

            // Continue discovering if encountering an empty cell
            if (BoardState.CellMineNumber(cl) == 0 && Difficulty.AutomaticDiscovery)
            {
                BoardState.ForAllSurroundingCells(cl, (p) => points.Add(p), Difficulty.WrapAround);
            }
            sum += 1;
        }

        if (mineHit) return LoseLife();

        return new GameState(BoardState.Discover(discoveredCells), ProgressState, Timing, View);
    }

    public GameState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardState.CellIsDiscovered(BoardState.Cursor)) return NotifyFailedAction();

        return new(BoardState.ToggleFlag(),ProgressState, Timing, View);
    }

    public GameState MoveCursor(Direction d) => new(BoardState.MoveCursor(d), ProgressState, Timing, View);

    public List<Point> CompareForVisibleChanges(GameState other)
    {
        Rectangle area = View.VisibleBoardSection;
        return BoardState.CompareForVisibleChanges(other.BoardState, area); 
    }

    private GameState PlaceMines() => new(BoardState.PlaceMines(), ProgressState.SetState(PlayerState.Playing), Timing.Resume(), View);

    public GameState SetPlayerState(PlayerState state) => new(BoardState, ProgressState.SetState(state), Timing, View);

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

    public bool ScrollIsNeeded => View.ScrollSafezone.Contains(BoardState.Cursor);

    public GameState Scroll()
    {
        BoardViewState newView = View.ScrollTo(BoardState.Cursor);

        return new(BoardState, ProgressState, Timing, newView);
    }
}
