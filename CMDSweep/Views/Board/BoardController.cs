using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;
using System;
using System.Collections.Generic;
using System.Timers;

namespace CMDSweep.Views.Board;

class BoardController : IViewController
{
    private readonly Timer refreshTimer;
    private readonly IRenderer _renderer;
    private BoardVisualizer _visualizer;

    public BoardState CurrentState { get; private set; }
    public GameApp App { get; }

    public BoardController(GameApp app)
    {
        App = app;
        _renderer = App.Renderer;
        if (SaveData.PlayerName == null) SaveData.PlayerName = "You";

        CurrentState = BoardState.NewGame(SaveData.CurrentDifficulty, Settings);

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += RefreshTimerElapsed;
        refreshTimer.AutoReset = true;

        _visualizer = new BoardVisualizer(_renderer, Settings, CurrentState);
    }

    public GameSettings Settings => App.Settings;

    public SaveData SaveData => App.SaveData;

    private void RefreshTimerElapsed(object? sender, ElapsedEventArgs e) => App.Refresh(RefreshMode.ChangesOnly);
    
    public bool Step()
    {
        InputAction ia = App.ReadAction();

        switch (ia)
        {
            case InputAction.NewGame:
                NewGame();
                return true;

            case InputAction.Help:
                refreshTimer.Stop();
                App.ShowHelp();
                return true;

            case InputAction.Quit:
                refreshTimer.Stop();
                App.ShowMainMenu();
                return true;

            default:
                return ProcessBoardChangeInput(ia);
        }
    }

    private bool ProcessBoardChangeInput(InputAction ia)
    {
        if (App.AppState == ApplicationState.Done)
        {
            NewGame(); // Todo: check if this flow still makes sense
            return true;
        }

        CurrentState = ia switch
        {
            InputAction.Up => CurrentState.MoveCursor(Direction.Up),
            InputAction.Down => CurrentState.MoveCursor(Direction.Down),
            InputAction.Left => CurrentState.MoveCursor(Direction.Left),
            InputAction.Right => CurrentState.MoveCursor(Direction.Right),
            InputAction.Dig => CurrentState.Dig(),
            InputAction.Flag => CurrentState.ToggleFlag(),
            _ => CurrentState
        };

        AfterStepStateChanges();
        return true;
    }

    private void AfterStepStateChanges()
    {
        switch (CurrentState.RoundState.PlayerState)
        {
            case PlayerState.Playing:
                if (!refreshTimer.Enabled) refreshTimer.Start();
                App.Refresh(RefreshMode.ChangesOnly);
                break;

            case PlayerState.Dead:
            case PlayerState.Win:
                refreshTimer.Stop();
                if (CurrentState.RoundState.PlayerState == PlayerState.Win)
                    CheckHighscoreFlow(CurrentState);

                App.AppState = ApplicationState.Done;
                App.Refresh(RefreshMode.Full);
                break;

            default:
                App.Refresh(RefreshMode.ChangesOnly);
                break;
        }
    }

    private BoardState CheckHighscoreFlow(BoardState currentState)
    {
        TimeSpan time = currentState.Timing.Time;
        if (currentState.TimeMakesHighscore())
        {
            currentState = currentState.SetPlayerState(PlayerState.EnteringHighscore);
            App.Refresh(RefreshMode.Full);

            bool textActive = true;
            while (textActive)
            {
                InputAction ia = App.ParseAction(HighscoreTextField.Activate());
                switch (ia)
                {
                    case InputAction.Dig:
                    case InputAction.Quit:
                        textActive = false;
                        break;

                    default:
                        break;
                }
            }

            currentState = currentState.SetPlayerState(PlayerState.ShowingHighscores);
            AddHighscore(time);
            App.Refresh(RefreshMode.Full);
        }
        return currentState;
    }

    private void AddHighscore(TimeSpan time)
    {
        SaveData.PlayerName = HighscoreTextField.Text;
        List<HighscoreRecord> scores = SaveData.CurrentDifficulty.Highscores;

        while (scores.Count >= HighscoreTable.highscoreEntries)
            scores.RemoveAt(HighscoreTable.highscoreEntries - 1);

        scores.Add(new()
        {
            Time = time,
            Name = HighscoreTextField.Text,
            Date = DateTime.Now
        });

        scores.Sort((x, y) => (x.Time - y.Time).Milliseconds);
        Storage.WriteSave(SaveData);
    }

    internal void NewGame()
    {
        ResetGameState();

        App.ChangeMode(ApplicationState.Playing);
    }

    private void ResetGameState()
    {
        refreshTimer.Stop();
        CurrentState = BoardState.NewGame(SaveData.CurrentDifficulty, Settings);
        Storage.WriteSave(SaveData);
    }

    public bool CheckScroll() => !CurrentState!.ScrollIsNeeded;

    public void Scroll() => CurrentState = CurrentState!.Scroll();

    public bool CheckResize()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension();
        return !newRenderMask.Equals(CurrentState!.View.RenderMask);
    }

    public void ResizeView()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension(); // Area that the board can be drawn into
        CurrentState!.View.ChangeRenderMask(newRenderMask);
    }

    private Rectangle RenderMaskFromConsoleDimension()
    {
        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        Rectangle newRenderMask = _renderer.Bounds.Shrink(0, barheight, 0, 0);
        return newRenderMask;
    }

    public void Refresh(RefreshMode mode)
    {
        throw new NotImplementedException();
    }
}