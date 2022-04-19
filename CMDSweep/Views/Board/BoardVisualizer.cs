using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

class BoardVisualizer : Visualizer<BoardState>
{
    private int OffsetX => RenderMask.Left - Viewport.Left * ScaleX;
    private int OffsetY => RenderMask.Top - Viewport.Top * ScaleY;

    private int ScaleX = 1;
    private int ScaleY = 1;

    Rectangle ScrollValidMask; // the area that is still valid (board space)
    Rectangle RenderMask; // the area the board can be drawn into (screen space)
    Rectangle Viewport; // rendermask mapped to (board space)

    public BoardVisualizer(BoardController bctrl) : base(bctrl)
    {
        HideStyle = Settings.GetStyle("cell-bg-out-of-bounds", "cell-bg-out-of-bounds");

        RenderMask = Rectangle.Zero;
        Viewport = Rectangle.Zero;
        ScrollValidMask = Rectangle.Zero;
    }
    internal override BoardState RetrieveState() => ((BoardController)Controller).CurrentState;
    internal override bool CheckFullRefresh() => CurrentState!.RoundData.PlayerState != LastState!.RoundData.PlayerState;

    private void RenderHighscorePopup(BoardState curGS)
    {
        TableGrid tg = Highscores.GetHSTableGrid(Settings);
        tg.CenterOn(Renderer.Bounds.Center);

        RenderPopupBox(Settings.GetStyle("popup"), tg.Bounds.Grow(2), "popup-border");
        Highscores.RenderHSTable(Renderer, Settings, tg, curGS.Difficulty, Settings.GetStyle("popup"));
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
        changes = CurrentState!.CompareForChanges(LastState!, Viewport);
        foreach (Point cl in changes) RenderCell(cl);
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
        string face = CurrentState!.RoundData.Face switch
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
        for (int i = 0; i < CurrentState!.Difficulty.Lives - CurrentState!.RoundData.LivesLost; i++) atext += life + " ";

        string btext = "";
        for (int i = 0; i < CurrentState!.RoundData.LivesLost; i++) btext += life + " ";

        Renderer.PrintAtTile(p, livesLeftStyle, atext);
        Renderer.PrintAtTile(p.Shifted(atext.Length, 0), livesGoneStyle, btext);
    }

    Rectangle ScrollSafeZone => Viewport.Shrink(Settings.Dimensions["scroll-safezone"]);

    internal override bool CheckScroll() => !ScrollSafeZone.Contains(CurrentState!.RoundData.Cursor);

    internal override void Scroll()
    {
        // Change the offset
        Offset offset = ScrollSafeZone.OffsetOutOfBounds(CurrentState!.RoundData.Cursor);
        Rectangle nvp = Viewport.Shift(offset);
        ScrollValidMask = ScrollValidMask.Intersect(nvp);

        Rectangle oldArea = MapToRender(ScrollValidMask);
        Viewport.Shift(offset);
        if (oldArea.Area > 0) Renderer.CopyArea(oldArea, MapToRender(ScrollValidMask));

        Viewport.ForAll(p => { if (!ScrollValidMask.Contains(p)) RenderViewPortCell(p); });
        RenderBorder();

        ScrollValidMask = Viewport;
    }

    private void RenderViewPortCell(Point p)
    {
        if (CurrentState!.BoardData.Bounds.Contains(p)) RenderCell(p);
        else if (CurrentState!.BoardData.Bounds.Grow(1).Contains(p)) RenderBorderCell(p);
        else ClearCell(p);
    }

    private void RenderBorderCell(Point p)
    {
        StyleData data = Settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        // Corners
        if (p.Equals(new Point(-1, -1)))
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-corner-tl"]);
        else if (p.Equals(new Point(CurrentState!.BoardData.BoardWidth, -1)))
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-corner-tr"]);
        else if (p.Equals(new Point(-1, CurrentState!.BoardData.BoardHeight)))
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-corner-bl"]);
        else if (p.Equals(new Point(CurrentState!.BoardData.BoardWidth, CurrentState!.BoardData.BoardHeight)))
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-corner-br"]);

        // Edges
        else if (p.Y == -1 || p.Y == CurrentState!.BoardData.BoardHeight)
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-horizontal"]);
        else if (p.X == -1 || p.X == CurrentState!.BoardData.BoardWidth)
            MappedPrint(p.X, p.Y, data, Settings.Texts["border-vertical"]);
    }

    private void ClearCell(Point p) => MappedPrint(p, HideStyle, "  ");

    private Rectangle MapToRender(Rectangle r) => new(MapToRender(r.TopLeft), MapToRender(r.BottomRight));
    private Point MapToRender(Point p) => new(OffsetX + p.X * ScaleX, OffsetY + p.Y * ScaleY);


    internal override bool CheckResize()
    {
        ScaleX = Settings.Dimensions["cell-size-x"];
        ScaleY = Settings.Dimensions["cell-size-y"];

        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        Rectangle consoleBounds = Renderer.Bounds; // Whole Console
        Rectangle newRenderMask = consoleBounds.Shrink(0, barheight, 0, 0); // Area that the board can be drawn into

        // Return if the measurements did not change
        return !newRenderMask.Equals(newRenderMask);
    }

    internal override void Resize()
    {

        ScaleX = Settings.Dimensions["cell-size-x"];
        ScaleY = Settings.Dimensions["cell-size-y"];

        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        Rectangle newRenderMask = Renderer.Bounds.Shrink(0, barheight, 0, 0); // Area that the board can be drawn into

        // Reset render shortcuts
        RenderMask = newRenderMask;
        ScrollValidMask = Rectangle.Zero;

        // Create a new viewport to fit
        Rectangle newVP = new(Viewport.Left, Viewport.Top, RenderMask.Width / ScaleX, RenderMask.Height / ScaleY);

        // Align the new viewport as best as we can
        if (Viewport.Equals(Rectangle.Zero))
            newVP.CenterOn(CurrentState!.BoardData.Bounds.Center);
        else
            newVP.CenterOn(Viewport.Center);

        Viewport = newVP;
    }

    internal override void RenderFull()
    {
        TryCenterViewPort();
        // Border
        Renderer.ClearScreen(HideStyle);
        RenderBorder();

        // Tiles
        Viewport.Intersect(CurrentState!.BoardData.Bounds).ForAll((x, y) => RenderCell(new Point(x, y)));

        // Extras
        RenderStatBoard();
        RenderMessages();

        Renderer.HideCursor(HideStyle);
        ScrollValidMask = Viewport;
    }

    private void RenderMessages()
    {
        if (CurrentState!.RoundData.PlayerState == PlayerState.Win)
            RenderPopup("Congratulations, You won!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundData.PlayerState == PlayerState.Dead)
            RenderPopup("You died!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundData.PlayerState == PlayerState.ShowingHighscores)
            RenderHighscorePopup(CurrentState);

        if (CurrentState.RoundData.PlayerState == PlayerState.EnteringHighscore)
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

    private void TryCenterViewPort()
    {
        if (CurrentState!.BoardData.BoardWidth < ScrollSafeZone.Width && CurrentState!.BoardData.BoardHeight < ScrollSafeZone.Height)
        {
            Viewport.CenterOn(CurrentState!.BoardData.Bounds.Center);
        }
    }

    void MappedPrint(Point p, StyleData data, string s)
    {
        if (Viewport.Contains(p))
            Renderer.PrintAtTile(MapToRender(p), data, s);
    }
    void MappedPrint(int x, int y, StyleData data, string s) => MappedPrint(new Point(x, y), data, s);

    void RenderBorder()
    {
        StyleData data = Settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        // Top
        MappedPrint(-1, -1, data, Settings.Texts["border-corner-tl"]);
        for (int x = 0; x < CurrentState!.BoardData.BoardWidth; x++) MappedPrint(x, -1, data, Settings.Texts["border-horizontal"]);
        MappedPrint(CurrentState!.BoardData.BoardWidth, -1, data, Settings.Texts["border-corner-tr"]);

        // Sides
        for (int y = 0; y < CurrentState!.BoardData.BoardHeight; y++)
        {
            MappedPrint(-1, y, data, Settings.Texts["border-vertical"]);
            MappedPrint(CurrentState!.BoardData.BoardWidth, y, data, Settings.Texts["border-vertical"]);
        }

        // Bottom
        MappedPrint(-1, CurrentState!.BoardData.BoardHeight, data, Settings.Texts["border-corner-bl"]);
        for (int x = 0; x < CurrentState!.BoardData.BoardWidth; x++) MappedPrint(x, CurrentState!.BoardData.BoardHeight, data, Settings.Texts["border-horizontal"]);
        MappedPrint(CurrentState!.BoardData.BoardWidth, CurrentState!.BoardData.BoardHeight, data, Settings.Texts["border-corner-br"]);
    }

    void RenderCell(Point cl)
    {
        ConsoleColor fg = GetTileVisual(cl) switch
        {
            TileVisual.Discovered => Settings.Colors["cell-fg-discovered"],
            TileVisual.Undiscovered => Settings.Colors["cell-fg-undiscovered"],
            TileVisual.UndiscoveredGrid => Settings.Colors["cell-fg-undiscovered-grid"],
            TileVisual.Flagged => Settings.Colors["cell-flagged"],
            TileVisual.DiscoveredMine => Settings.Colors["cell-mine-discovered"],
            TileVisual.DeadWrongFlag => Settings.Colors["cell-dead-wrong-flag"],
            TileVisual.DeadMine => Settings.Colors["cell-dead-mine-missed"],
            TileVisual.DeadMineExploded => Settings.Colors["cell-dead-mine-hit"],
            TileVisual.DeadMineFlagged => Settings.Colors["cell-dead-mine-flagged"],
            TileVisual.DeadDiscovered => Settings.Colors["cell-fg-discovered"],
            TileVisual.DeadUndiscovered => Settings.Colors["cell-fg-undiscovered"],
            TileVisual.QuestionMarked => Settings.Colors["cell-questionmarked"],
            _ => Settings.Colors["cell-fg-out-of-bounds"],
        };

        string text = Settings.Texts["cell-empty"];
        switch (GetTileVisual(cl))
        {
            case TileVisual.Undiscovered:
            case TileVisual.DeadUndiscovered:
            case TileVisual.UndiscoveredGrid:
                text = Settings.Texts["cell-undiscovered"];
                break;

            case TileVisual.DeadWrongFlag:
            case TileVisual.DeadMineFlagged:
            case TileVisual.Flagged:
                text = Settings.Texts["cell-flag"];
                break;

            case TileVisual.DeadMine:
            case TileVisual.DiscoveredMine:
            case TileVisual.DeadMineExploded:
                text = Settings.Texts["cell-mine"];
                break;

            case TileVisual.QuestionMarked:
                text = Settings.Texts["cell-questionmarked"];
                break;

            case TileVisual.DeadDiscovered:
            case TileVisual.Discovered:
                int num = CurrentState!.BoardData.CellMineNumber(cl);
                if (CurrentState!.Difficulty.SubtractFlags) num = CurrentState!.BoardData.CellSubtractedMineNumber(cl);
                if (num > 0 && (CurrentState!.RoundData.Cursor.Equals(cl) || !CurrentState!.Difficulty.OnlyShowAtCursor))
                {
                    text = num.ToString();
                    fg = Settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
                }
                else
                {
                    text = Settings.Texts["cell-empty"];
                }
                break;
        }

        // Cursor
        if (CurrentState!.RoundData.PlayerState != PlayerState.Dead && CurrentState!.RoundData.Cursor.Equals(cl))
        {
            if (!CurrentState!.Difficulty.OnlyShowAtCursor || GetTileVisual(cl) != TileVisual.Discovered || CurrentState!.BoardData.CellMineNumber(cl) <= 0)
                fg = Settings.Colors["cell-selected"];
            if (text == Settings.Texts["cell-undiscovered"] || text == Settings.Texts["cell-empty"])
                text = Settings.Texts["cursor"];
        }

        ConsoleColor bg = GetTileVisual(cl) switch
        {
            TileVisual.Discovered or
            TileVisual.DeadDiscovered or
            TileVisual.DeadMineExploded or
            TileVisual.DiscoveredMine => Settings.Colors["cell-bg-discovered"],

            TileVisual.UndiscoveredGrid => Settings.Colors["cell-bg-undiscovered-grid"],

            _ => Settings.Colors["cell-bg-undiscovered"]
        };

        StyleData data = new(fg, bg, false);

        // Padding
        int padRight = 0;
        if (text.Length < ScaleX) padRight = ScaleX - text.Length;
        int padLeft = padRight / 2;
        padRight = ScaleX - padLeft;

        // Actual rendering
        MappedPrint(cl, data, " ".PadLeft(padLeft));
        MappedPrint(cl, data, text.PadRight(padRight));
    }

    private TileVisual GetTileVisual(Point cl)
    {
        BoardState gs = CurrentState!;

        if (gs.RoundData.PlayerState == PlayerState.Dead)
        {
            if (gs.BoardData.CellIsMine(cl))
            {
                if (gs.BoardData.CellIsFlagged(cl)) return TileVisual.DeadMineFlagged;
                if (gs.BoardData.CellIsDiscovered(cl)) return TileVisual.DeadMineExploded;
                else return TileVisual.DeadMine;
            }
            else
            {
                if (gs.BoardData.CellIsFlagged(cl)) return TileVisual.DeadWrongFlag;
                if (gs.BoardData.CellIsDiscovered(cl)) return TileVisual.DeadDiscovered;
                else return TileVisual.DeadUndiscovered;
            }
        }
        else
        {
            if (gs.BoardData.CellIsDiscovered(cl) && gs.BoardData.CellIsMine(cl)) return TileVisual.DiscoveredMine;
            if (gs.BoardData.CellIsDiscovered(cl)) return TileVisual.Discovered;
            if (gs.BoardData.CellIsFlagged(cl)) return TileVisual.Flagged;
            if (gs.BoardData.CellIsQuestionMarked(cl)) return TileVisual.QuestionMarked;
            if (cl.X % Settings.Dimensions["cell-grid-size"] == 0) return TileVisual.UndiscoveredGrid;
            if (cl.Y % Settings.Dimensions["cell-grid-size"] == 0) return TileVisual.UndiscoveredGrid;
            else return TileVisual.Undiscovered;
        }
    }

    enum TileVisual
    {
        Undiscovered,
        Flagged,
        QuestionMarked,
        Discovered,
        DiscoveredMine,

        DeadUndiscovered,
        DeadDiscovered,
        DeadMine,
        DeadMineExploded,
        DeadMineFlagged,
        DeadWrongFlag,
        UndiscoveredGrid,
    }
}
