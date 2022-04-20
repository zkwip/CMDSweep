using CMDSweep.Data;
using CMDSweep.IO;
using CMDSweep.Layout;
using CMDSweep.Views.Board.State;
using System;
using System.Collections.Generic;
using System.Timers;

namespace CMDSweep.Views.Board;

class BoardController : Controller
{
    public BoardState CurrentState;
    private readonly Timer refreshTimer;
    public TextEnterField HighscoreTextField;

    public BoardController(GameApp g) : base(g)
    {
        Visualizer = new BoardVisualizer(this);

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += RefreshTimerElapsed;
        refreshTimer.AutoReset = true;

        if (SaveData.PlayerName == null) SaveData.PlayerName = "You";
        HighscoreTextField = new TextEnterField(this, new(0, 0, 15, 1), App.Renderer, Settings.GetStyle("popup-textbox")) { Text = SaveData.PlayerName };
    }
    
    private void RefreshTimerElapsed(object? sender, ElapsedEventArgs e) => App.Refresh(RefreshMode.ChangesOnly);
    
    internal override bool Step()
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
                return ProcessBoardChange(ia);
        }
    }

    private bool ProcessBoardChange(InputAction ia)
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

        while (scores.Count >= Highscores.highscoreEntries)
            scores.RemoveAt(Highscores.highscoreEntries - 1);

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
        refreshTimer.Stop();
        CurrentState = BoardState.NewGame(SaveData.CurrentDifficulty, Settings);
        Storage.WriteSave(SaveData);
        App.ChangeMode(ApplicationState.Playing);
    }


}