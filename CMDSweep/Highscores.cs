using System;
using System.Collections.Generic;

namespace CMDSweep;

internal class HighscoreController : Controller
{
    internal Difficulty SelectedDifficulty;
    public HighscoreController(GameApp app) : base(app) 
    {
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
                App.BControl.InitialiseGame();
                break;
        }
        return true;
    }
    internal void ShowHighscores() => App.AppState = ApplicationState.Highscore;
}
internal static class Highscores
{

    internal const int highscoreEntries = 5;
    internal static TableGrid GetHSTableGrid(GameSettings settings)
    {
        var dims = settings.Dimensions;
        TableGrid tg = new();
        tg.AddColumn(dims["popup-padding-x"], 0, "");
        tg.AddColumn(dims["highscore-num-width"], 0, "num");
        tg.AddColumn(dims["highscore-name-width"], 0, "name");
        tg.AddColumn(dims["highscore-time-width"], 0, "time");
        tg.AddColumn(dims["highscore-date-width"], 0, "date");
        tg.AddColumn(dims["popup-padding-x"], 0, "");

        tg.AddRow(dims["highscore-header-height"], 0, "title");
        tg.AddRow(dims["popup-padding-y"], 0, "");
        tg.AddRow(dims["highscore-header-height"], 0, "head");
        tg.AddRow(dims["highscore-row-height"], 0, "row", Highscores.highscoreEntries);
        tg.FitAround(0);

        return tg;
    }

    internal static void RenderHSTable(IRenderer renderer, GameSettings settings, TableGrid tg, Difficulty dif, StyleData style)
    {
        List<HighscoreRecord> highscores = dif.Highscores;

        renderer.PrintAtTile(tg.GetPoint("num", "title"), style, "Highscores for " + dif.Name);
        renderer.PrintAtTile(tg.GetPoint("num", "head"), style, "#");
        renderer.PrintAtTile(tg.GetPoint("name", "head"), style, "Name");
        renderer.PrintAtTile(tg.GetPoint("time", "head"), style, "Time");
        renderer.PrintAtTile(tg.GetPoint("date", "head"), style, "When");

        for (int i = 0; i < highscores.Count; i++)
        {
            string time = String.Format(
                "{0:D3}:{1:D2}.{2:D3}",
                (int)(highscores[i].Time.TotalMinutes),
                highscores[i].Time.Seconds,
                highscores[i].Time.Milliseconds
            );

            StyleData rowstyle = style;

            string date = highscores[i].Date.ToString("g");
            if (DateTime.Now - highscores[i].Date < TimeSpan.FromSeconds(5))
            {
                date = "Now";
                rowstyle = new StyleData(settings.Colors["popup-fg-highlight"], style.Background);
            }
            else if (highscores[i].Date.Date == DateTime.Today)
            {
                date = "today " + highscores[i].Date.ToString("t");
            }

            renderer.PrintAtTile(tg.GetPoint("num", 0, "row", i), rowstyle, (i + 1).ToString());
            renderer.PrintAtTile(tg.GetPoint("name", 0, "row", i), rowstyle, highscores[i].Name);
            renderer.PrintAtTile(tg.GetPoint("time", 0, "row", i), rowstyle, time);
            renderer.PrintAtTile(tg.GetPoint("date", 0, "row", i), rowstyle, date);
        }
    }
}

internal class HighscoreVisualizer : Visualizer<Difficulty>
{
    TableGrid ScoreTable;
    public HighscoreVisualizer(HighscoreController hctrl)
    {
        Controller = hctrl;
        hideStyle = Settings.GetStyle("menu");
    }

    internal override bool CheckFullRefresh() => true;

    internal override bool CheckResize() => true;

    internal override bool CheckScroll() => false;

    internal override void RenderChanges() => RenderFull();

    internal override void RenderFull()
    {
        Renderer.ClearScreen(hideStyle);
        Highscores.RenderHSTable(Renderer, Settings, ScoreTable, CurrentState!, hideStyle);
    }

    internal override void Resize()
    {
        ScoreTable = Highscores.GetHSTableGrid(Settings);
        ScoreTable.CenterOn(Renderer.Bounds.Center);
    }

    internal override Difficulty RetrieveState() => ((HighscoreController)Controller).SelectedDifficulty;

    internal override void Scroll() { }
}
