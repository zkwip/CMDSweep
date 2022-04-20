using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Board.State;

internal record class BoardState
{
    // Properties
    public readonly BoardData BoardData;
    public readonly RoundStats RoundStats;
    public readonly Timing Timing;
    public readonly BoardView View;

    public BoardState(BoardData boardData, RoundStats roundStats, Timing timing, BoardView view)
    {
        BoardData = boardData;
        RoundStats = roundStats;
        Timing = timing;
        View = view;
    }

    internal static BoardState NewGame(Difficulty diff, GameSettings settings)
    {
        BoardData boardData = BoardData.NewGame(diff);
        RoundStats roundData = RoundStats.NewGame(diff);

        BoardView view = new BoardView(settings,boardData.Bounds);
        Timing timing = Timing.NewGame(diff);

        return new BoardState(boardData, roundData, timing, view);
    }

    public Difficulty Difficulty => RoundStats.Difficulty;

    public int MinesLeft => RoundStats.Mines - BoardData.Flags - RoundStats.LivesLost;

    public double MineRate => (double)(RoundStats.LivesLost + BoardData.Flags) / RoundStats.Mines;

    public BoardState Win() => new(BoardData, RoundStats.Win(), Timing.Stop(),View);

    public BoardState LoseLife()
    {
        NotifyFailedAction();

        if (RoundStats.CanLoseLife)
            return new BoardState(BoardData, RoundStats.LoseLife(), Timing, View);

        return new BoardState(BoardData, RoundStats.Die(), Timing.Stop(), View);
    }

    public BoardState ResumeGame() => new(BoardData, RoundStats, Timing.Resume(), View);

    public BoardState FreezeGame() => new(BoardData, RoundStats, Timing.Pause(), View);

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

        if (RoundStats.PlayerState == PlayerState.NewGame) 
            return PlaceMines().Discover(BoardData.Cursor).CheckForWin();

        return Discover(BoardData.Cursor).CheckForWin();
    }

    private BoardState CheckForWin()
    {
        if (BoardData.Discovered + RoundStats.Mines - RoundStats.LivesLost == BoardData.Tiles) 
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

        return new BoardState(BoardData.Discover(discoveredCells), RoundStats, Timing, View);
    }

    public BoardState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardData.CellIsDiscovered(BoardData.Cursor)) return NotifyFailedAction();

        return new(BoardData.ToggleFlag(),RoundStats, Timing, View);
    }

    public BoardState MoveCursor(Direction d) => new(BoardData.MoveCursor(d), RoundStats, Timing, View);

    public List<Point> CompareForVisibleChanges(BoardState other)
    {
        Rectangle area = View.VisibleBoardSection;
        return BoardData.CompareForVisibleChanges(other.BoardData, area); 
    }

    private BoardState PlaceMines() => new(BoardData.PlaceMines(), RoundStats.SetState(PlayerState.Playing), Timing.Resume(), View);

    public BoardState SetPlayerState(PlayerState state) => new(BoardData, RoundStats.SetState(state), Timing, View);

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

    public bool ScrollIsNeeded => View.ScrollSafezone.Contains(BoardData.Cursor);

    public (BoardState, RenderBufferCopyTask) Scroll()
    {
        BoardView oldView = View;
        BoardView newView = oldView.ScrollTo(BoardData.Cursor);

        Rectangle oldCopyArea = oldView.MapToRender(newView.ScrollValidMask);
        Rectangle newCopyArea = newView.MapToRender(newView.ScrollValidMask);

        RenderBufferCopyTask task = new(oldCopyArea, newCopyArea);

        return (new(BoardData,RoundStats,Timing,newView), task);
    }
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
