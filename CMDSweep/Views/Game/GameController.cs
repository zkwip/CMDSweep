using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.IO;
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
    private readonly GameVisualizer _visualizer;
    private readonly RenderSheduler<GameState> _renderSheduler;
    private string _enteredName;

    public GameState CurrentState { get; private set; }

    public MineApp App { get; }

    public GameController(MineApp app)
    {
        App = app;
        _renderer = App.Renderer;

        if (SaveData.PlayerName == null) 
            SaveData.PlayerName = "You";

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += RefreshTimerElapsed;
        refreshTimer.AutoReset = true;

        CurrentState = PrepareNewGameState();

        _visualizer = new GameVisualizer(_renderer, Settings);
        _renderSheduler = new RenderSheduler<GameState>(_visualizer, _renderer);
        _enteredName = SaveData.PlayerName;
    }

    public GameSettings Settings => App.Settings;

    public SaveData SaveData => App.SaveData;

    private void RefreshTimerElapsed(object? sender, ElapsedEventArgs e) => App.Refresh(RefreshMode.ChangesOnly);
    
    public void Step()
    {
        switch (CurrentState.ProgressState.PlayerState)
        {
            case PlayerState.Playing:
            case PlayerState.NewGame:
                PlayStep();
                break;

            case PlayerState.EnteringHighscore:
                EnterHighscoreStep();
                break;

            case PlayerState.ShowingHighscores:
            case PlayerState.Dead:
            case PlayerState.Win:
                GameEndStep();
                break;
        }
    }

    private void EnterHighscoreStep() 
    {
        (string name, bool done) = ConsoleTextInputReader.HandleTypingKeyPress(false, SaveData.PlayerName);

        SaveData.PlayerName = name;

        if (done)
        {
            AddHighscore(CurrentState.Timing.Time);
            CurrentState.SetPlayerState(PlayerState.ShowingHighscores);
        }
    }

    private void GameEndStep() 
    {
        InputAction ia = App.ReadAction();

        switch (ia)
        {
            case InputAction.Help:
                App.ShowHelp();
                return;

            case InputAction.Quit:
                refreshTimer.Stop();
                App.ShowMainMenu();
                return;

            case InputAction.Dig:
                NewGame();
                return;

            default:
                return;
        }
    }

    private void PlayStep()
    {
        InputAction ia = App.ReadAction();

        switch (ia)
        {
            case InputAction.NewGame:
                NewGame();
                return;

            case InputAction.Help:
                refreshTimer.Stop();
                App.ShowHelp();
                return;

            case InputAction.Quit:
                refreshTimer.Stop();
                App.ShowMainMenu();
                return;

            default:
                HandleBoardTransitionInput(ia);
                return;
        }
    }

    private bool HandleBoardTransitionInput(InputAction ia)
    {
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

        if (CurrentState.ProgressState.PlayerState == PlayerState.Playing) 
        { 
            if (!refreshTimer.Enabled)
                refreshTimer.Start();
        }

        refreshTimer.Stop();
        return true;
    }

    private void AddHighscore(TimeSpan time)
    {
        SaveData.PlayerName = _enteredName;
        List<HighscoreRecord> scores = SaveData.CurrentDifficulty.Highscores;

        while (scores.Count >= HighscoreTable.highscoreEntries)
            scores.RemoveAt(HighscoreTable.highscoreEntries - 1);

        scores.Add(new()
        {
            Time = time,
            Name = _enteredName,
            Date = DateTime.Now
        });

        scores.Sort((x, y) => (x.Time - y.Time).Milliseconds);
        Storage.WriteSave(SaveData);
    }

    internal void NewGame()
    {
        CurrentState = PrepareNewGameState();
        App.ChangeMode(ApplicationState.Playing);
    }

    private GameState PrepareNewGameState() => GameState.NewGame(SaveData.CurrentDifficulty, Settings, RenderMaskFromConsoleDimension());

    public void ResizeView() => CurrentState = CurrentState.ChangeRenderMask(RenderMaskFromConsoleDimension());

    private Rectangle RenderMaskFromConsoleDimension()
    {
        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        return _renderer.Bounds.Shrink(0, barheight, 0, 0);
    }

    public void Refresh(RefreshMode mode) => _renderSheduler.Refresh(CurrentState, mode);

}