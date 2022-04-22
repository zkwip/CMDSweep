using CMDSweep.Data;
using System;

namespace CMDSweep.Views.Highscore;

class HighscoreController : IViewController
{
    private HighscoreVisualizer _visualizer;
    internal Difficulty SelectedDifficulty;

    public GameApp App { get; }

    public HighscoreController(GameApp app)
    {
        App = app;

        _visualizer = new HighscoreVisualizer(app.Renderer, app.Settings);
        SelectedDifficulty = app.SaveData.CurrentDifficulty;
    }

    public bool Step()
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

    public void ShowHighscores() => App.AppState = ApplicationState.Highscore;

    public void Refresh(RefreshMode _) => _visualizer.Visualize(SelectedDifficulty);

    public void ResizeView() => _visualizer.Resize(); 
}
