﻿using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game.State;

internal record class GameState
{
    // Properties
    public readonly BoardState BoardState;
    public readonly GameProgressState ProgressState;
    public readonly TimingState Timing;

    public GameState(BoardState boardData, GameProgressState roundStats, TimingState timing)
    {
        BoardState = boardData;
        ProgressState = roundStats;
        Timing = timing;
    }

    internal static GameState NewGame(Difficulty diff, GameSettings settings, Rectangle boardRenderMask)
    {
        BoardState boardState = BoardState.NewGame(diff, settings, boardRenderMask);
        GameProgressState progressState = GameProgressState.NewGame(diff);
        TimingState timing = TimingState.NewGame(diff);

        return new GameState(boardState, progressState, timing);
    }

    public Difficulty Difficulty => ProgressState.Difficulty;

    public int MinesLeft => ProgressState.Mines - BoardState.Flags - ProgressState.LivesLost;

    public double MineProgressRatio => (double)(ProgressState.LivesLost + BoardState.Flags) / ProgressState.Mines;

    public GameState Win() => new(BoardState, ProgressState.Win(), Timing.Stop());

    public GameState LoseLife()
    {
        NotifyFailedAction();

        if (ProgressState.CanLoseLife)
            return new GameState(BoardState, ProgressState.LoseLife(), Timing);

        return new GameState(BoardState, ProgressState.Die(), Timing.Stop());
    }

    public GameState ResumeGame() => new(BoardState, ProgressState, Timing.Resume());

    public GameState FreezeGame() => new(BoardState, ProgressState, Timing.Pause());

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

        return new GameState(BoardState.Discover(discoveredCells), ProgressState, Timing);
    }

    public GameState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardState.CellIsDiscovered(BoardState.Cursor)) return NotifyFailedAction();

        return new(BoardState.ToggleFlag(),ProgressState, Timing);
    }

    public GameState MoveCursor(Direction d) => new(BoardState.MoveCursor(d), ProgressState, Timing);

    private GameState PlaceMines() => new(BoardState.PlaceMines(), ProgressState.SetState(PlayerState.Playing), Timing.Resume());

    public GameState SetPlayerState(PlayerState state) => new(BoardState, ProgressState.SetState(state), Timing);

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
}
