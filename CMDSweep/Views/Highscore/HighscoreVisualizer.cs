using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Highscore;

class HighscoreVisualizer : ITypeVisualizer<Difficulty>
{
    private TableGrid _scoreTable;
    private IRenderer _renderer;
    private StyleData _styleData;
    private GameSettings _gameSettings;

    public HighscoreVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _styleData = settings.GetStyle("menu");
        Resize();
    }

    public void Visualize(Difficulty difficulty)
    {
        _renderer.ClearScreen(_styleData);
        HighscoreTable.RenderHSTable(_renderer, _gameSettings, _scoreTable, difficulty, _styleData);
    }

    public void Resize()
    {
        _scoreTable = HighscoreTable.GetHSTableGrid(_gameSettings);
        _scoreTable.CenterOn(_renderer.Bounds.Center);
    }
}
