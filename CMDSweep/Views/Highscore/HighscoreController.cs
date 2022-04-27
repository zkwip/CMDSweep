using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Highscore;

class HighscoreController : IViewController
{
    private HighscoreVisualizer _visualizer;
    internal Difficulty SelectedDifficulty;

    public MineApp App { get; }

    public HighscoreController(MineApp app)
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
                App.MenuController.OpenMenu(App.MenuController.MainMenu);
                break;
            case InputAction.NewGame:
                App.GameController.NewGame();
                break;
        }
        return true;
    }

    public void ShowHighscores() => App.AppState = ApplicationState.Highscore;

    public void Refresh(RefreshMode _) => _visualizer.Visualize(SelectedDifficulty);

    public void ResizeView() => _visualizer.Resize(); 
}
