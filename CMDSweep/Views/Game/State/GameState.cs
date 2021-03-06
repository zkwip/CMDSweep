using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Layout.Text;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game.State;

internal record class GameState : IRenderState
{
    // Properties
    private readonly int _id;
    public readonly BoardState BoardState;
    public readonly TimingState Timing;
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly int Lives;
    public readonly Face Face;
    public readonly TextEnterDialog EnteredNameDialog;

    public GameState(BoardState boardData, TimingState timing, Difficulty difficulty, PlayerState playerState, int lives, Face face, TextEnterDialog dialog, int id)
    {
        BoardState = boardData;
        Timing = timing;
        Difficulty = difficulty;
        PlayerState = playerState;
        Lives = lives;
        Face = face;
        _id = id;
        EnteredNameDialog = dialog;
    }

    public static GameState NewGame(SaveData save, GameSettings settings, Rectangle boardRenderMask)
    {
        BoardState boardState = BoardState.NewGame(save.CurrentDifficulty, settings, boardRenderMask);
        TimingState timing = TimingState.NewGame();
        TextEnterDialog enterDialog = new(settings.Texts["popup-enter-hs-message"], save.PlayerName, settings.Dimensions["popup-enter-hs-width"], settings.Dimensions["popup-enter-hs-height"]);

        return new GameState(
            boardState,
            timing,
            save.CurrentDifficulty,
            PlayerState.NewGame,
            save.CurrentDifficulty.Lives,
            Face.Normal,
            enterDialog,
            0);
    }

    public int Id => _id;

    public int MinesLeft => Mines - BoardState.Flags - LivesLost;

    public double MineProgressRatio => (double)(LivesLost + BoardState.Flags) / Mines;


    public GameState TryLoseLife()
    {
        NotifyFailedAction();

        if (CanLoseLife)
            return LoseLife();
        return Die();
    }

    public GameState ResumeGame() => new(BoardState, Timing.Resume(), Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);

    internal GameState SetEnteredName(string name) => new(BoardState, Timing, Difficulty, PlayerState, Lives, Face, EnteredNameDialog.UpdateValue(name), _id + 1);

    public GameState FreezeGame() => new(BoardState, Timing.Pause(), Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);

    public GameState NotifyFailedAction()
    {
        Console.Beep();
        return this;
    }

    public GameState Dig()
    {
        if (BoardState.CellIsDiscovered(BoardState.Cursor))
            return this;

        if (BoardState.CellIsFlagged(BoardState.Cursor))
            return NotifyFailedAction();


        GameState res = this;

        if (PlayerState == PlayerState.NewGame)
            res = PlaceMines();

        res = res.Discover(BoardState.Cursor);

        res = res.CheckForWin();

        return res;
    }

    private GameState CheckForWin()
    {
        if (BoardState.Discovered + Mines - LivesLost == BoardState.Tiles)
            return Win();
        return this;
    }

    public GameState Discover(Point cl)
    {
        List<Point> frontierQueue = new();
        List<Point> discoveredCells = new();

        frontierQueue.Add(cl);
        bool mineHit = false;

        while (frontierQueue.Count > 0)
        {
            // Pop
            cl = frontierQueue[0];
            frontierQueue.RemoveAt(0);

            // Check discoverable
            if (discoveredCells.Contains(cl)) 
                continue;

            if (BoardState.CellOutsideBounds(cl)) 
                continue;

            if (BoardState.CellIsDiscovered(cl)) 
                continue;

            if (BoardState.CellIsFlagged(cl)) 
                continue;


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
        }

        if (mineHit)
            return TryLoseLife();

        return new GameState(BoardState.Discover(discoveredCells), Timing, Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);
    }

    public GameState ToggleFlag()
    {
        if (!Difficulty.FlagsAllowed) return NotifyFailedAction();
        if (BoardState.CellIsDiscovered(BoardState.Cursor)) return NotifyFailedAction();

        return new(BoardState.ToggleFlag(), Timing, Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);
    }

    public GameState MoveCursor(Direction d) => new(BoardState.MoveCursor(d), Timing, Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);

    private GameState PlaceMines() => new(BoardState.PlaceMines(), Timing.Resume(), Difficulty, PlayerState.Playing, Lives, Face, EnteredNameDialog, _id + 1);

    public GameState ChangeRenderMask(Rectangle newRenderMask) => new(BoardState.ChangeRenderMask(newRenderMask), Timing, Difficulty, PlayerState, Lives, Face, EnteredNameDialog, _id + 1);

    public int Mines => Difficulty.Mines;

    public int LivesLost => Difficulty.Lives - Lives;

    public bool CanLoseLife => Lives > 1;

    public bool Dead => PlayerState == PlayerState.Dead;

    public GameState Win() => new(BoardState, Timing.Pause(), Difficulty, PlayerState.Win, Lives, Face.Win, EnteredNameDialog, _id + 1);

    public GameState LoseLife() => new(BoardState, Timing, Difficulty, PlayerState, Lives - 1, Face, EnteredNameDialog, _id + 1);

    public GameState Die() => new(BoardState, Timing.Pause(), Difficulty, PlayerState.Dead, 0, Face.Dead, EnteredNameDialog, _id + 1);

    public GameState SetPlayerState(PlayerState state) => new(BoardState, Timing, Difficulty, state, Lives, Face, EnteredNameDialog, _id + 1);
}
