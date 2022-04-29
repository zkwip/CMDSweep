using CMDSweep.Geometry;
using CMDSweep.Layout;
using System;
using System.Collections.Generic;

namespace CMDSweep.Data;

class HighscoreTable : IPlaceable
{
    internal const int highscoreEntries = 5;
    private readonly string _name;
    private readonly List<HighscoreRecord> _highscores;
    private readonly TableGrid _tableGrid;

    private Dimensions _dimensions;

    public HighscoreTable(Difficulty difficulty, GameSettings settings)
    {
        _name = difficulty.Name;
        _highscores = difficulty.Highscores;

        _tableGrid = new();
        BuildTableGrid(settings);
    }

    private void BuildTableGrid(GameSettings settings)
    {
        var dims = settings.Dimensions;

        _tableGrid.AddColumn(dims["popup-padding-x"], 0, "");
        _tableGrid.AddColumn(dims["highscore-num-width"], 0, "num");
        _tableGrid.AddColumn(dims["highscore-name-width"], 0, "name");
        _tableGrid.AddColumn(dims["highscore-time-width"], 0, "time");
        _tableGrid.AddColumn(dims["highscore-date-width"], 0, "date");
        _tableGrid.AddColumn(dims["popup-padding-x"], 0, "");

        _tableGrid.AddRow(dims["highscore-header-height"], 0, "title");
        _tableGrid.AddRow(dims["popup-padding-y"], 0, "");
        _tableGrid.AddRow(dims["highscore-header-height"], 0, "head");
        _tableGrid.AddRow(dims["highscore-row-height"], 0, "row", highscoreEntries);

        _dimensions = _tableGrid.ContentFitDimensions(0);
    }

    public string Name => _name;

    public Dimensions ContentDimensions => _dimensions;

    public TableGrid Grid => _tableGrid;

    public string Time(int i)
    {
        return string.Format(
                "{0:D3}:{1:D2}.{2:D3}",
                (int)_highscores[i].Time.TotalMinutes,
                _highscores[i].Time.Seconds,
                _highscores[i].Time.Milliseconds
            );
    }

    public string Date(int i)
    {
        if (IsNow(i))
            return "Now";

        if (IsToday(i))
            return "today " + _highscores[i].Date.ToString("t");

        return _highscores[i].Date.ToString("g");
    }

    public bool IsNow(int i) => DateTime.Now - _highscores[i].Date < TimeSpan.FromSeconds(5);

    private bool IsToday(int i) => _highscores[i].Date.Date == DateTime.Today;

    public string PlayerName(int i) => _highscores[i].Name;

    public int Count => _highscores.Count;
}
