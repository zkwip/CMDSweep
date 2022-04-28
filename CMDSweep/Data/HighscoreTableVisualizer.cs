using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Data;

class HighscoreTableVisualizer : ITypeVisualizer<HighscoreTable, Rectangle>
{
    private readonly IRenderer _renderer;
    private readonly StyleData _normalStyle;
    private readonly StyleData _nowStyle;

    public HighscoreTableVisualizer(IRenderer renderer, StyleData nowStyle, StyleData normalStyle)
    {
        _renderer = renderer;
        _nowStyle = nowStyle;
        _normalStyle = normalStyle;
    }

    public void Visualize(HighscoreTable table, Rectangle bounds)
    {
        table.Grid.Bounds = bounds;
        _renderer.ClearScreen(_nowStyle, bounds);

        _renderer.PrintAtTile(table.Grid.GetPoint("num", "title"), _normalStyle, "Highscores for " + table.Name);
        _renderer.PrintAtTile(table.Grid.GetPoint("num", "head"), _normalStyle, "#");
        _renderer.PrintAtTile(table.Grid.GetPoint("name", "head"), _normalStyle, "Name");
        _renderer.PrintAtTile(table.Grid.GetPoint("time", "head"), _normalStyle, "Time");
        _renderer.PrintAtTile(table.Grid.GetPoint("date", "head"), _normalStyle, "When");

        for (int i = 0; i < HighscoreTable.highscoreEntries; i++)
        {
            StyleData rowstyle = table.IsNow(i) ? _normalStyle : _nowStyle;
            _renderer.PrintAtTile(table.Grid.GetPoint("num", 0, "row", i), rowstyle, (i + 1).ToString());
            _renderer.PrintAtTile(table.Grid.GetPoint("name", 0, "row", i), rowstyle, table.PlayerName(i));
            _renderer.PrintAtTile(table.Grid.GetPoint("time", 0, "row", i), rowstyle, table.Time(i));
            _renderer.PrintAtTile(table.Grid.GetPoint("date", 0, "row", i), rowstyle, table.Date(i));
        }
    }
}
