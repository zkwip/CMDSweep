﻿using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game.State;

internal record class GameState : IRenderState
{
    // Properties
    public readonly BoardState BoardState;
    public readonly GameProgressState ProgressState;
    public readonly TimingState Timing;
    private readonly int _id;

    public GameState(BoardState boardData, GameProgressState roundStats, TimingState timing, int id)
    {
        BoardState = boardData;
        ProgressState = roundStats;
        Timing = timing;
        _id = id;
    }

    public static GameState NewGame(Difficulty diff, GameSettings settings, Rectangle boardRenderMask)
    {
        BoardState boardState = BoardState.NewGame(diff, settings, boardRenderMask);
        GameProgressState progressState = GameProgressState.NewGame(diff);
        TimingState timing = TimingState.NewGame();

        return new GameState(boardState, progressState, timing, 0);
    }

    public int Id => _id;

    public Difficulty Difficulty => ProgressState.Difficulty;

    public int MinesLeft => ProgressState.Mines - BoardState.Flags - ProgressState.LivesLost;

    public double MineProgressRatio => (double)(ProgressState.LivesLost + BoardState.Flags) / ProgressState.Mines;

    public GameState Win() => new(BoardState, ProgressState.Win(), Timing.Pause(), _id+1);

    public GameState LoseLife()
    {
        NotifyFailedAction();

        if (ProgressState.CanLoseLife)
            return new GameState(BoardState, ProgressState.LoseLife(), Timing, _id + 1);

        return new GameState(BoardState, ProgressState.Die(), Timing.Pause(), _id + 1);
    }

    public GameState ResumeGame() => new(BoardState, ProgressState, Timing.Resume(), _id + 1);

    public GameState FreezeGame() => new(BoardState, ProgressState, Timing.Pause(), _id + 1);

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


        GameState res = this; 

        if (ProgressState.PlayerState == PlayerState.NewGame)
            res = PlaceMines();

        res = res.Discover(BoardState.Cursor);

        res = res.CheckForWin();

        return res;
    }

    private GameState CheckForWin()
    {
        if (BoardState.Discovered + ProgressState.Mines - ProgressState.LivesLost == BoardState.Tiles) 
            return Win();
        return this;
    }

    public GameState Discover(Point cl)
    {
        List<Point> frontierQueue = new();
        List<Point> discoveredCells = new();

        frontierQueue.Add(cl);

        int sum = 0;
        bool mineHit = false;

        while (frontierQueue.Count > 0)
        {
            // Pop
            cl = frontierQueue[0];
            frontierQueue.RemoveAt(0);

            // Check discoverable
            if (BoardState.CellOutsideBounds(cl)) continue;
            if (BoardState.CellIsDiscovered(cl)) continue;
            if (BoardState.CellIsFlagged(cl)) continue;
            if (discoveredCells.Contains(cl)) continue;

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
                BoardState.ForAllSurroundingCells(cl, (p) => frontierQueue.Add(p), Difficulty.WrapAround);
            }
            sum += 1;
        }

        if (mineHit) 
            return LoseLife();

        return new GameState(BoardState.Discover(discoveredCells), ProgressState, Timing, _id + 1);
    }

    public GameState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardState.CellIsDiscovered(BoardState.Cursor)) return NotifyFailedAction();

        return new(BoardState.ToggleFlag(),ProgressState, Timing, _id + 1);
    }

    public GameState MoveCursor(Direction d) => new(BoardState.MoveCursor(d), ProgressState, Timing, _id + 1);

    private GameState PlaceMines() => new(BoardState.PlaceMines(), ProgressState.SetState(PlayerState.Playing), Timing.Resume(), _id + 1);

    public GameState SetPlayerState(PlayerState state) => new(BoardState, ProgressState.SetState(state), Timing, _id + 1);

    public bool TimeMakesHighscore()
    {
        TimeSpan time = Timing.Time;
        List<HighscoreRecord> scores = Difficulty.Highscores;

        if (scores.Count >= HighscoreTable.highscoreEntries)
        {
            if (time < scores[HighscoreTable.highscoreEntries - 1].Time)
                return true;
            return false;
        }
        return true;
    }

    public GameState ChangeRenderMask(Rectangle newRenderMask) => new(BoardState.ChangeRenderMask(newRenderMask), ProgressState, Timing, _id + 1);
}
