using System;
using System.Collections.Generic;

namespace CMDSweep;

internal class HighscoreController : Controller
{
    public HighscoreController(GameApp app) : base(app) 
    {
        Visualizer = new HighscoreVisualizer(app);
    }

    private HighscoreVisualizer Visualizer;

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

    internal override void Visualize(RefreshMode mode) => Visualizer.Visualize(mode);

    internal void ShowHighscores() => App.appState = ApplicationState.Highscore;
}
internal static class Highscores
{

    internal const int highscoreEntries = 5;
    internal static TableGrid GetHSTableGrid(GameApp game)
    {
        var dims = game.Settings.Dimensions;
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

    internal static void RenderHSTable(GameApp game, TableGrid tg, Difficulty dif, StyleData style)
    {
        IRenderer renderer = game.Renderer;
        List<HighscoreRecord> highscores = dif.Highscores;
        GameSettings settings = game.Settings;

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

internal class HighscoreVisualizer
{
    private readonly GameApp game;
    private readonly StyleData menuStyle;

    private IRenderer renderer => game.Renderer;
    
    internal Difficulty? CurrentDiff;
    public HighscoreVisualizer(GameApp gameApp)
    {
        this.game = gameApp;
        menuStyle = gameApp.Settings.GetStyle("menu");
        CurrentDiff = game.SaveData.CurrentDifficulty;
    }

    internal void Visualize(RefreshMode mode)
    {
        if (CurrentDiff == null) throw new ArgumentNullException("No difficulty set");

        renderer.ClearScreen(menuStyle);
        TableGrid tg = Highscores.GetHSTableGrid(game);
        tg.CenterOn(renderer.Bounds.Center);
        Highscores.RenderHSTable(game, tg, CurrentDiff, menuStyle);
    }
}
