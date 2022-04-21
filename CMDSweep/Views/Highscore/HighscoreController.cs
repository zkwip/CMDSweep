using CMDSweep.Data;
using System;

namespace CMDSweep.Views.Highscore;

class HighscoreController : IViewController
{
    internal Difficulty SelectedDifficulty;

    public GameApp App { get; }

    public HighscoreController(GameApp app)
    {
        App = app;

        Visualizer = new HighscoreVisualizer(this);
        SelectedDifficulty = app.SaveData.CurrentDifficulty;
    }

    internal override bool Step()
    {
        InputAction ia = App.ReadAction();
        switch (ia)
        {
            case InputAction.Quit:
                App.MControl.OpenMenu(App.MControl.MainMenu);
                break;
            case InputAction.NewGame:
                App.BControl.NewGame();
                break;
        }
        return true;
    }
    internal void ShowHighscores() => App.AppState = ApplicationState.Highscore;

    public void Visualize(RefreshMode mode)
    {
        throw new NotImplementedException();
    }

    bool IViewController.Step()
    {
        throw new NotImplementedException();
    }
}
