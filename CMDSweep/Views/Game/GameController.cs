using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;
using System;
using System.Collections.Generic;
using System.Timers;

namespace CMDSweep.Views.Game;

class GameController : IViewController
{
    private readonly Timer refreshTimer;
    private readonly IRenderer _renderer;
    private GameVisualizer _visualizer;
    private RenderSheduler<GameState> _renderSheduler;

    private TextEnterField _highscoreTextField;

    public GameState CurrentState { get; private set; }

    public MineApp App { get; }

    public GameController(MineApp app)
    {
        App = app;
        _renderer = App.Renderer;

        if (SaveData.PlayerName == null) 
            SaveData.PlayerName = "You";

        CurrentState = GameState.NewGame(SaveData.CurrentDifficulty, Settings);

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += RefreshTimerElapsed;
        refreshTimer.AutoReset = true;

        _visualizer = new GameVisualizer(_renderer, Settings, CurrentState);

        _renderSheduler = new RenderSheduler<GameState>(_visualizer, _renderer);
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
        switch (CurrentState.ProgressState.PlayerState)
        {
            case PlayerState.Playing:
                if (!refreshTimer.Enabled) refreshTimer.Start();
                App.Refresh(RefreshMode.ChangesOnly);
                break;

            case PlayerState.Dead:
            case PlayerState.Win:
                refreshTimer.Stop();
                if (CurrentState.ProgressState.PlayerState == PlayerState.Win)
                    CheckHighscoreFlow(CurrentState);

                App.AppState = ApplicationState.Done;
                App.Refresh(RefreshMode.Full);
                break;

            default:
                App.Refresh(RefreshMode.ChangesOnly);
                break;
        }
    }

    private GameState CheckHighscoreFlow(GameState currentState)
    {
        TimeSpan time = currentState.Timing.Time;
        if (currentState.TimeMakesHighscore())
        {
            currentState = currentState.SetPlayerState(PlayerState.EnteringHighscore);
            App.Refresh(RefreshMode.Full);

            bool textActive = true;
            while (textActive)
            {
                InputAction ia = App.ParseAction(_highscoreTextField.HandleInput());
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
        SaveData.PlayerName = _highscoreTextField.Text;
        List<HighscoreRecord> scores = SaveData.CurrentDifficulty.Highscores;

        while (scores.Count >= HighscoreTable.highscoreEntries)
            scores.RemoveAt(HighscoreTable.highscoreEntries - 1);

        scores.Add(new()
        {
            Time = time,
            Name = _highscoreTextField.Text,
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
        CurrentState = GameState.NewGame(SaveData.CurrentDifficulty, Settings);
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

    public void Refresh(RefreshMode mode) => _renderSheduler.Visualize(CurrentState, mode);

}