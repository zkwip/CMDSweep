using System;
using System.Collections.Generic;
using System.Timers;
namespace CMDSweep;

internal class BoardController : Controller
{
    internal BoardState CurrentState;
    private readonly Timer refreshTimer;

    internal BoardController(GameApp g) : base(g)
    {
        Visualizer = new BoardVisualizer(this);

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += RefreshTimerElapsed;
        refreshTimer.AutoReset = true;
    }
    private void RefreshTimerElapsed(object? sender, ElapsedEventArgs e) => App.Refresh(RefreshMode.ChangesOnly);
    internal override bool Step()
    {
        if (App.AppState == ApplicationState.Done) return DoneStep();
        InputAction ia = App.ReadAction();
        CurrentState.Face = Face.Normal;

        if (ia == InputAction.NewGame) InitialiseGame();
        else if (ia == InputAction.Quit)
        {
            refreshTimer.Stop();
            App.MControl.OpenMenu(App.MControl.MainMenu);
        }
        else
        {
            // Process actions

            CurrentState = CurrentState.Clone();
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

            if (CurrentState.PlayerState == PlayerState.Playing)
            {
                if (!refreshTimer.Enabled) refreshTimer.Start();
                App.Refresh(RefreshMode.ChangesOnly);
            }
            else if (CurrentState.PlayerState == PlayerState.Dead || CurrentState.PlayerState == PlayerState.Win)
            {
                refreshTimer.Stop();

                if (CurrentState.PlayerState == PlayerState.Win)
                    CurrentState.highscore = CheckHighscore(CurrentState);

                App.Refresh(RefreshMode.Full);
                App.AppState = ApplicationState.Done;
            }
            else
            {
                App.Refresh(RefreshMode.ChangesOnly);
            }
        }
        return true;
    }

    internal bool DoneStep()
    {
        InputAction ia = App.ReadAction();
        switch (ia)
        {
            case InputAction.Quit:
                App.MControl.OpenMenu(App.MControl.MainMenu);
                break;
            case InputAction.Dig:
            case InputAction.NewGame:
                InitialiseGame();
                break;
        }
        return true;
    }

    private bool CheckHighscore(BoardState currentState)
    {
        TimeSpan time = currentState.Time;
        List<HighscoreRecord> scores = App.SaveData.CurrentDifficulty.Highscores;

        if (scores.Count >= Highscores.highscoreEntries)
        {
            if (time < scores[Highscores.highscoreEntries - 1].Time) scores.RemoveAt(Highscores.highscoreEntries - 1);
        }

        if (scores.Count < Highscores.highscoreEntries)
        {
            scores.Add(new()
            {
                Time = time,
                Name = "Test",
                Date = DateTime.Now
            });

            scores.Sort((x, y) => (x.Time - y.Time).Milliseconds);
            Storage.WriteSave(App.SaveData);

            return true;
        }
        return false;
    }
    internal void InitialiseGame()
    {
        refreshTimer.Stop();
        App.AppState = ApplicationState.Playing;

        CurrentState = BoardState.NewGame(App.SaveData.CurrentDifficulty);
        Storage.WriteSave(App.SaveData);

        App.Refresh(RefreshMode.Full);
    }
}