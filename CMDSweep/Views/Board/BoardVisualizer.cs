using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

partial class BoardVisualizer : Visualizer<BoardState>
{
    TileStyling _tileStyling;
    BoardBorderStyling _borderStyling;
    public BoardVisualizer(BoardController bctrl) : base(bctrl)
    {
        HideStyle = Settings.GetStyle("cell-bg-out-of-bounds", "cell-bg-out-of-bounds");
        _tileStyling = new TileStyling();
        _borderStyling = new BoardBorderStyling();
    }

    internal override BoardState RetrieveState() => ((BoardController)Controller).CurrentState;
    
    internal override bool CheckFullRefresh() => CurrentState!.RoundStats.PlayerState != LastState!.RoundStats.PlayerState;

    internal override bool CheckScroll() => !CurrentState!.ScrollIsNeeded;
    internal override void Scroll()
    {
        RenderBufferCopyTask task = CurrentState!.Scroll();
        Renderer.CopyArea(task);

        CurrentState!.View.Viewport.ForAll(p => { 
            if (!CurrentState!.View.IsScrollValid(p)) 
                RenderViewPortCell(p); 
            }
        );

        //RenderBorder();
    }

    private void RenderHighscorePopup(BoardState curGS)
    {
        TableGrid tableGrid = Highscores.GetHSTableGrid(Settings);
        tableGrid.CenterOn(Renderer.Bounds.Center);

        RenderPopupBox(Settings.GetStyle("popup"), tableGrid.Bounds.Grow(2), "popup-border");
        Highscores.RenderHSTable(Renderer, Settings, tableGrid, curGS.Difficulty, Settings.GetStyle("popup"));
    }

    private void RenderPopup(string text)
    {
        int xpad = Settings.Dimensions["popup-padding-x"];
        int ypad = Settings.Dimensions["popup-padding-y"];
        StyleData style = Settings.GetStyle("popup");

        TextRenderBox textbox = new(text, Renderer.Bounds.Shrink(xpad, ypad, xpad, ypad));
        textbox.HorizontalAlign = HorzontalAlignment.Center;
        textbox.Bounds = textbox.Used;
        RenderPopupAroundShape(textbox.Bounds);
        textbox.Render(Renderer, style, false);

    }

    private void RenderPopupAroundShape(Rectangle rect)
    {
        int xpad = Settings.Dimensions["popup-padding-x"];
        int ypad = Settings.Dimensions["popup-padding-y"];
        StyleData style = Settings.GetStyle("popup");
        rect.CenterOn(Renderer.Bounds.Center);
        RenderPopupBox(style, rect.Grow(xpad, ypad, xpad, ypad), "popup-border");
    }

    private void RenderPopupBox(StyleData style, Rectangle r, string border)
    {
        Renderer.ClearScreen(style, r);

        r = r.Shrink(0, 0, 1, 1); // since it is exclusive
        r.HorizontalRange.ForEach((i) => Renderer.PrintAtTile(new(i, r.Top), style, Settings.Texts[border + "-side-top"]));
        r.HorizontalRange.ForEach((i) => Renderer.PrintAtTile(new(i, r.Bottom), style, Settings.Texts[border + "-side-bottom"]));

        r.VerticalRange.ForEach((i) => Renderer.PrintAtTile(new(r.Left, i), style, Settings.Texts[border + "-side-left"]));
        r.VerticalRange.ForEach((i) => Renderer.PrintAtTile(new(r.Right, i), style, Settings.Texts[border + "-side-right"]));

        Renderer.PrintAtTile(r.TopLeft, style, Settings.Texts[border + "-corner-tl"]);
        Renderer.PrintAtTile(r.BottomLeft, style, Settings.Texts[border + "-corner-bl"]);
        Renderer.PrintAtTile(r.TopRight, style, Settings.Texts[border + "-corner-tr"]);
        Renderer.PrintAtTile(r.BottomRight, style, Settings.Texts[border + "-corner-br"]);

    }

    internal override void RenderChanges()
    {
        List<Point> changes;
        changes = CurrentState!.CompareForVisibleChanges(LastState!);
        foreach (Point cl in changes) RenderBoardCell(cl);
        RenderStatBoard();
    }

    private void RenderStatBoard()
    {
        TableGrid bar = new();
        bar.Bounds = new(Renderer.Bounds.HorizontalRange, LinearRange.Zero);

        int horpad = Settings.Dimensions["stat-padding-x"];
        int verpad = Settings.Dimensions["stat-padding-y"];
        int vmidpad = Settings.Dimensions["stat-padding-x-in-between"];

        // Rows
        bar.AddRow(verpad, 0);
        bar.AddRow(1, 0, "bar");
        bar.AddRow(verpad, 0);

        // Columns
        bar.AddColumn(horpad, 0);
        bar.AddColumn(6, 0, "clock");
        bar.AddColumn(vmidpad + 5, 0);
        bar.AddColumn(horpad, 1);

        bar.AddColumn(4, 0, "face");

        bar.AddColumn(horpad, 1);
        bar.AddColumn(5, 0, "lives");
        bar.AddColumn(vmidpad, 0);
        bar.AddColumn(6, 0, "mines");
        bar.AddColumn(horpad, 0);

        Renderer.ClearScreen(HideStyle, bar.Bounds);

        RenderClock(bar.GetPoint("clock", "bar"));
        RenderFace(bar.GetPoint("face", "bar"));
        RenderLifeCounter(bar.GetPoint("lives", "bar"));
        RenderMineCounter(bar.GetPoint("mines", "bar"));

    }

    private void RenderClock(Point p)
    {
        StyleData clockStyle = Settings.GetStyle("stat-mines");
        Renderer.PrintAtTile(p, clockStyle, CurrentState!.Timing.Time.ToString(@"\ h\:mm\:ss\ "));
    }

    private void RenderFace(Point p)
    {
        string face = CurrentState!.RoundStats.Face switch
        {
            Face.Surprise => Settings.Texts["face-surprise"],
            Face.Win => Settings.Texts["face-win"],
            Face.Dead => Settings.Texts["face-dead"],
            _ => Settings.Texts["face-normal"],
        };

        StyleData faceStyle = Settings.GetStyle("face");
        Renderer.PrintAtTile(p, faceStyle, face);
    }

    private void RenderMineCounter(Point p)
    {
        StyleData minesLeftStyle = Settings.GetStyle("stat-mines");
        Renderer.PrintAtTile(p, minesLeftStyle, string.Format(" {0:D3} ", CurrentState!.MinesLeft));
    }

    private void RenderLifeCounter(Point p)
    {
        char life = Settings.Texts["stat-life"][0];
        StyleData livesLeftStyle = Settings.GetStyle("stat-mines");
        StyleData livesGoneStyle = Settings.GetStyle("stat-lives-lost", "stat-mines-bg");

        string atext = " ";
        for (int i = 0; i < CurrentState!.Difficulty.Lives - CurrentState!.RoundStats.LivesLost; i++) atext += life + " ";

        string btext = "";
        for (int i = 0; i < CurrentState!.RoundStats.LivesLost; i++) btext += life + " ";

        Renderer.PrintAtTile(p, livesLeftStyle, atext);
        Renderer.PrintAtTile(p.Shifted(atext.Length, 0), livesGoneStyle, btext);
    }

    

    private void RenderViewPortCell(Point p, bool clearOutside = true)
    {
        if (CurrentState!.BoardData.Bounds.Contains(p)) 
            RenderBoardCell(p);
        else if (CurrentState!.BoardData.Bounds.Grow(1).Contains(p)) 
            RenderBorderCell(p);
        else if (clearOutside) ClearCell(p);
    }

    private void RenderBorderCell(Point p) => MappedPrint(p, _borderStyling.GetBorderStyle(p));

    private void ClearCell(Point p) => MappedPrint(p, HideStyle, "  ");

    internal override bool CheckResize()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension();

        // Return if the measurements did not change
        return !newRenderMask.Equals(CurrentState!.View.RenderMask);
    }

    internal override void Resize()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension(); // Area that the board can be drawn into
        CurrentState!.View.ChangeRenderMask(newRenderMask);
    }

    private Rectangle RenderMaskFromConsoleDimension()
    {
        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        Rectangle newRenderMask = Renderer.Bounds.Shrink(0, barheight, 0, 0);
        return newRenderMask;
    }

    internal override void RenderFull()
    {
        CurrentState!.View.TryCenterViewPort();

        // Border
        Renderer.ClearScreen(HideStyle);
        CurrentState!.View.VisibleBoardSection.ForAll(p => RenderViewPortCell(p, false));

        // Extras
        RenderStatBoard();
        RenderMessages();

        Renderer.HideCursor(HideStyle);
        CurrentState!.View.ValidateViewPort();
    }

    private void RenderMessages()
    {
        if (CurrentState!.RoundStats.PlayerState == PlayerState.Win)
            RenderPopup("Congratulations, You won!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundStats.PlayerState == PlayerState.Dead)
            RenderPopup("You died!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundStats.PlayerState == PlayerState.ShowingHighscores)
            RenderHighscorePopup(CurrentState);

        if (CurrentState.RoundStats.PlayerState == PlayerState.EnteringHighscore)
            RenderNewHighscorePopup();

    }

    private void RenderNewHighscorePopup()
    {
        BoardController bc = (BoardController)Controller;
        TextEnterField tef = bc.HighscoreTextField;

        TableGrid tg = new();

        tg.AddColumn(Settings.Dimensions["popup-enter-hs-width"], 0);
        tg.AddRow(2, 0);
        tg.AddRow(1, 0);
        tg.FitAround(0);

        Rectangle shape = tg.Bounds;
        RenderPopupAroundShape(shape);
        tg.Bounds = shape;

        tef.Bounds = tg.GetCell(0, 1);
        TextRenderBox trb = new TextRenderBox(Settings.Texts["popup-enter-hs-message"], tg.GetCell(0, 0));
        trb.Render(Renderer, Settings.GetStyle("popup"), false);

    }

    void MappedPrint(Point p, StyleData data, string s)
    {
        if (CurrentState!.View.IsVisible(p))
            Renderer.PrintAtTile(CurrentState!.View.MapToRender(p), data, s);
    }
    void MappedPrint(int x, int y, StyleData data, string s) => MappedPrint(new Point(x, y), data, s);

    void MappedPrint(Point p, StyledText text) => MappedPrint(p, text.Style, text.Text);

    void RenderBoardCell(Point cl) => MappedPrint(cl, _tileStyling.CellVisual(cl));
}
