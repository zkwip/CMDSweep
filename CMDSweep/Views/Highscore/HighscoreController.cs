using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Highscore;

class HighscoreController : IViewController
{
    private ITypeVisualizer<HighscoreTable, Rectangle> _visualizer;
    private GameSettings _settings;
    internal Difficulty SelectedDifficulty;

    public MineApp App { get; }

    public HighscoreController(MineApp app)
    {
        App = app;
        _settings = App.Settings;

        StyleData normalStyle = App.Settings.GetStyle("menu");
        _visualizer = new HighscoreTableVisualizer(app.Renderer, normalStyle, normalStyle);
        SelectedDifficulty = app.SaveData.CurrentDifficulty;
    }

    public void Step()
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
    }

    public void ShowHighscores() => App.AppState = ApplicationState.Highscore;

    public void Refresh(RefreshMode _)
    {
        HighscoreTable table = new HighscoreTable(SelectedDifficulty, _settings);
        Rectangle bounds = Rectangle.Centered(App.Renderer.Bounds.Center, table.ContentDimensions);

        _visualizer.Visualize(table, bounds);
    }

    public void ResizeView() { }
}
