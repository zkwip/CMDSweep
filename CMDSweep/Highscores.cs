using System;
using System.Collections.Generic;

namespace CMDSweep;

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

    internal static void RenderHSTable(GameApp game, TableGrid tg, Difficulty dif)
    {
        IRenderer renderer = game.Renderer;
        List<HighscoreRecord> highscores = dif.Highscores;
        GameSettings settings = game.Settings;

        renderer.PrintAtTile(tg.GetPoint("num", "title"), settings.GetStyle("popup"), "Highscores for " + dif.Name);
        renderer.PrintAtTile(tg.GetPoint("num", "head"), settings.GetStyle("popup"), "#");
        renderer.PrintAtTile(tg.GetPoint("name", "head"), settings.GetStyle("popup"), "Name");
        renderer.PrintAtTile(tg.GetPoint("time", "head"), settings.GetStyle("popup"), "Time");
        renderer.PrintAtTile(tg.GetPoint("date", "head"), settings.GetStyle("popup"), "When");

        for (int i = 0; i < highscores.Count; i++)
        {
            string time = String.Format(
                "{0:D3}:{1:D2}.{2:D3}",
                (int)(highscores[i].Time.TotalMinutes),
                highscores[i].Time.Seconds,
                highscores[i].Time.Milliseconds
            );

            StyleData style = settings.GetStyle("popup");

            string date = highscores[i].Date.ToString("g");
            if (DateTime.Now - highscores[i].Date < TimeSpan.FromSeconds(5))
            {
                date = "Now";
                style = settings.GetStyle("popup-fg-highlight", "popup-bg");
            }
            else if (highscores[i].Date.Date == DateTime.Today)
            {
                date = "today " + highscores[i].Date.ToString("t");
            }

            renderer.PrintAtTile(tg.GetPoint("num", 0, "row", i), style, (i + 1).ToString());
            renderer.PrintAtTile(tg.GetPoint("name", 0, "row", i), style, highscores[i].Name);
            renderer.PrintAtTile(tg.GetPoint("time", 0, "row", i), style, time);
            renderer.PrintAtTile(tg.GetPoint("date", 0, "row", i), style, date);
        }
    }
}
