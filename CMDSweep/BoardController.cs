using System;
using System.Collections.Generic;
using System.Timers;
namespace CMDSweep;

class BoardController : Controller
{
    internal BoardState CurrentState;
    private readonly Timer refreshTimer;
    internal TextEnterField HighscoreTextField;

    internal BoardController(GameApp g) : base(g)
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
        CurrentState.Face = Face.Normal;

        if (ia == InputAction.NewGame) NewGame();
        else if (ia == InputAction.Help)
        {
            refreshTimer.Stop();
            App.ShowHelp();
        }
        else if (ia == InputAction.Quit)
        {
            refreshTimer.Stop();
            App.ShowMainMenu();
        }
        else if (App.AppState == ApplicationState.Done) 
        {
            NewGame(); // Todo: check if this flow still makes sense
        }
        else
        {
            // Process actions on this controller
            CurrentState = CurrentState.Clone();

            // Handle the keypresses as board actions
            switch (ia)
            {
                case InputAction.Up:
                    CurrentState.MoveCursor(Direction.Up);
                    break;
                case InputAction.Down:
                    CurrentState.MoveCursor(Direction.Down);
                    break;
                case InputAction.Left:
                    CurrentState.MoveCursor(Direction.Left);
                    break;
                case InputAction.Right:
                    CurrentState.MoveCursor(Direction.Right);
                    break;
                case InputAction.Dig:
                    CurrentState.Dig();
                    break;
                case InputAction.Flag:
                    CurrentState.ToggleFlag();
                    break;
            }

            // Determine what the consequences are for the game state and rendering
            AfterStepStateChanges();
        }
        return true;
    }

    private void AfterStepStateChanges()
    {
        switch (CurrentState.PlayerState)
        {
            case PlayerState.Playing:
                if (!refreshTimer.Enabled) refreshTimer.Start();
                App.Refresh(RefreshMode.ChangesOnly);
                break;

            case PlayerState.Dead:
            case PlayerState.Win:
                refreshTimer.Stop();
                if (CurrentState.PlayerState == PlayerState.Win) 
                    CheckHighscoreFlow(CurrentState);

                App.AppState = ApplicationState.Done;
                App.Refresh(RefreshMode.Full);
                break;

            default:
                App.Refresh(RefreshMode.ChangesOnly);
                break;
        }
    }

    private void CheckHighscoreFlow(BoardState currentState)
    {
        TimeSpan time = currentState.Time;
        if (CheckForHighscore(currentState))
        {
            currentState.PlayerState = PlayerState.EnteringHighscore;
            App.Refresh(RefreshMode.Full);

            bool textActive = true;
            while (textActive) { 
                InputAction ia  = App.ParseAction(HighscoreTextField.Activate());
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

            currentState.PlayerState = PlayerState.ShowingHighscores;
            AddHighscore(time);
            App.Refresh(RefreshMode.Full);
        }
    }

    private bool CheckForHighscore(BoardState currentState)
    {
        TimeSpan time = currentState.Time;
        List<HighscoreRecord> scores = SaveData.CurrentDifficulty.Highscores;

        if (scores.Count >= Highscores.highscoreEntries)
        {
            if (time < scores[Highscores.highscoreEntries - 1].Time)
                return true;
            else 
                return false;
        }
        return true;
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
        CurrentState = BoardState.NewGame(SaveData.CurrentDifficulty);
        Storage.WriteSave(SaveData);
        App.ChangeMode(ApplicationState.Playing);
    }
}