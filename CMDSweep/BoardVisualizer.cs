﻿using System;
using System.Collections.Generic;

namespace CMDSweep;

internal class BoardVisualizer
{
    IRenderer Renderer => Controller.App.Renderer;
    readonly BoardController Controller;
    readonly GameSettings settings;

    private int OffsetX => RenderMask.Left - Viewport.Left * ScaleX;
    private int OffsetY => RenderMask.Top - Viewport.Top * ScaleY;

    private int ScaleX = 1;
    private int ScaleY = 1;

    private bool IsRendering = false;
    private RefreshMode ModeWaiting = RefreshMode.None;

    Rectangle ScrollValidMask; // the area that is still valid (board space)
    Rectangle RenderMask; // the area the board can be drawn into (screen space)
    Rectangle Viewport; // rendermask mapped to (board space)

    private BoardState? lastRenderedGameState;
    private readonly StyleData hideStyle;

    public BoardVisualizer(BoardController c)
    {
        settings = c.App.Settings;
        Controller = c;
        hideStyle = settings.GetStyle("cell-bg-out-of-bounds", "cell-bg-out-of-bounds");
        RenderMask = Rectangle.Zero;
        Viewport = Rectangle.Zero;
        ScrollValidMask = Rectangle.Zero;
    }

    private void SetVisQueue(RefreshMode mode)
    {
        if (mode > ModeWaiting) ModeWaiting = mode;
    }
    public bool Visualize(RefreshMode mode)
    {
        // Indicate there is stuff to redraw
        SetVisQueue(mode);

        // Defer renderer
        if (IsRendering) return false;
        IsRendering = true;

        // Actual render loop
        while (ModeWaiting != RefreshMode.None)
        {
            lock (Renderer)
            {
                // Reset queue
                mode = ModeWaiting;
                ModeWaiting = RefreshMode.None;

                BoardState? prevGS = lastRenderedGameState;
                BoardState? curGS = Controller.CurrentState;

                // Decide what to render
                if (curGS == null)
                    continue; // Skip; Nothing to render

                if (UpdateOffsets(curGS))
                    mode = RefreshMode.Full; // Console changed size

                if (prevGS == null)
                    mode = RefreshMode.Full; // No history: Full render

                else if (mode == RefreshMode.ChangesOnly) // else to implicitly exclude the case where prevGS is null
                {
                    if (!CursorInScrollSafezone(curGS))
                        mode = RefreshMode.Scroll; // Scrolling
                    if (curGS.PlayerState != prevGS.PlayerState)
                        mode = RefreshMode.Full; // Player Mode changed
                }

                //Render
                if (mode == RefreshMode.Full) RenderFullBoard(curGS);
                else
                {
                    if (mode == RefreshMode.Scroll)
                        ScrollBoard(curGS);

                    RenderBoardChanges(curGS, prevGS!);
                }

                RenderStatBoard(curGS);

                if (curGS.PlayerState == PlayerState.Win)
                {
                    if (curGS.highscore)
                        RenderHighscorePopup(curGS);
                    else
                        RenderPopup("Congratulations, You won!\n\nYou can play again by pressing any key.");
                }
                if (curGS.PlayerState == PlayerState.Dead) RenderPopup("You died!\n\nYou can play again by pressing any key.");

                Renderer.HideCursor(hideStyle);
                lastRenderedGameState = curGS;
            }
        }

        IsRendering = false;
        return true;
    }

    private void RenderHighscorePopup(BoardState curGS)
    {
        TableGrid tg = Highscores.GetHSTableGrid(Controller.App);
        tg.CenterOn(Renderer.Bounds.Center);

        RenderPopupBox(settings.GetStyle("popup"), tg.Bounds.Grow(2),"popup-border");
        Highscores.RenderHSTable(Controller.App, tg, curGS.Difficulty, settings.GetStyle("popup"));
    }
    private void RenderPopup(string text)
    {
        int xpad = settings.Dimensions["popup-padding-x"];
        int ypad = settings.Dimensions["popup-padding-y"];
        StyleData style = settings.GetStyle("popup");

        int horRoom = Renderer.Bounds.Width - (4 * xpad);
        int verRoom = Renderer.Bounds.Height - (4 * ypad);

        List<String> lines = new(text.Split('\n'));
        int broadest = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            // Wrapping
            if (lines[i].Length > horRoom)
            {
                string line = lines[i];

                int breakpoint = horRoom;
                for (int j = 0; j < horRoom; j++) if (line[j] == ' ') breakpoint = j;

                lines.RemoveAt(i);
                lines.Insert(i, line[..breakpoint]);
                lines.Insert(i + 1, line[..(breakpoint + 1)]);
            }

            broadest = Math.Max(broadest, lines[i].Length);
        }

        Rectangle textbox = new(0, 0, broadest, lines.Count);
        textbox.CenterOn(Renderer.Bounds.Center);

        RenderPopupBox(style, textbox.Grow(xpad, ypad, xpad, ypad), "popup-border");

        for (int i = 0; i < lines.Count; i++)
        {
            Renderer.PrintAtTile(textbox.TopLeft.Shifted(0, i), style, lines[i]);
        }

    }

    private void RenderPopupBox(StyleData style, Rectangle r, string border)
    {
        Renderer.ClearScreen(style, r);

        r = r.Shrink(0,0,1,1); // since it is exclusive
        r.HorizontalRange.ForEach((i) => Renderer.PrintAtTile(new(i,r.Top), style, settings.Texts[border + "-side-top"]));
        r.HorizontalRange.ForEach((i) => Renderer.PrintAtTile(new(i, r.Bottom), style, settings.Texts[border + "-side-bottom"]));

        r.VerticalRange.ForEach((i) => Renderer.PrintAtTile(new(r.Left, i), style, settings.Texts[border + "-side-left"]));
        r.VerticalRange.ForEach((i) => Renderer.PrintAtTile(new(r.Right, i), style, settings.Texts[border + "-side-right"]));

        Renderer.PrintAtTile(r.TopLeft, style, settings.Texts[border + "-corner-tl"]);
        Renderer.PrintAtTile(r.BottomLeft, style, settings.Texts[border + "-corner-bl"]);
        Renderer.PrintAtTile(r.TopRight, style, settings.Texts[border + "-corner-tr"]);
        Renderer.PrintAtTile(r.BottomRight, style, settings.Texts[border + "-corner-br"]);

    }

    private void RenderBoardChanges(BoardState curGS, BoardState prevGS)
    {
        List<Point> changes;
        changes = curGS.CompareForChanges(prevGS, Viewport);
        foreach (Point cl in changes) RenderCell(cl, curGS);
    }

    private void RenderStatBoard(BoardState currentGS)
    {
        TableGrid bar = new();
        bar.Bounds = new(Renderer.Bounds.HorizontalRange, LinearRange.Zero);

        int horpad = settings.Dimensions["stat-padding-x"];
        int verpad = settings.Dimensions["stat-padding-y"];
        int vmidpad = settings.Dimensions["stat-padding-x-in-between"];

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

        Renderer.ClearScreen(hideStyle, bar.Bounds);

        RenderClock(bar.GetPoint("clock","bar"), currentGS);
        RenderFace(bar.GetPoint("face", "bar"), currentGS);
        RenderLifeCounter(bar.GetPoint("lives", "bar"), currentGS);
        RenderMineCounter(bar.GetPoint("mines", "bar"), currentGS);

    }

    private void RenderClock(Point p, BoardState currentGS)
    {
        StyleData clockStyle = settings.GetStyle("stat-mines");
        Renderer.PrintAtTile(p, clockStyle, currentGS.Time.ToString(@"\ h\:mm\:ss\ "));
    }

    private void RenderFace(Point p, BoardState currentGS)
    {
        string face = currentGS.Face switch
        {
            Face.Surprise => settings.Texts["face-surprise"],
            Face.Win => settings.Texts["face-win"],
            Face.Dead => settings.Texts["face-dead"],
            _ => settings.Texts["face-normal"],
        };
        StyleData faceStyle = settings.GetStyle("face");
        Renderer.PrintAtTile(p, faceStyle, face);
    }

    private void RenderMineCounter(Point p, BoardState currentGS)
    {
        StyleData minesLeftStyle = settings.GetStyle("stat-mines");
        Renderer.PrintAtTile(p, minesLeftStyle, string.Format(" {0:D3} ", currentGS.MinesLeft));
    }

    private void RenderLifeCounter(Point p, BoardState currentGS)
    {
        char life = settings.Texts["stat-life"][0];
        StyleData livesLeftStyle = settings.GetStyle("stat-mines");
        StyleData livesGoneStyle = settings.GetStyle("stat-lives-lost","stat-mines-bg");

        string atext = " ";
        for (int i = 0; i < currentGS.Difficulty.Lives - currentGS.LivesLost; i++) atext += life + " ";

        string btext = "";
        for (int i = 0; i < currentGS.LivesLost; i++) btext += life + " ";

        Renderer.PrintAtTile(p, livesLeftStyle, atext);
        Renderer.PrintAtTile(p.Shifted(atext.Length, 0), livesGoneStyle, btext);
    }
    Rectangle ScrollSafeZone => Viewport.Shrink(settings.Dimensions["scroll-safezone"]);
    bool CursorInScrollSafezone(BoardState gs) => ScrollSafeZone.Contains(gs.Cursor);
    bool ScrollBoard(BoardState gs)
    {
        // Change the offset
        Offset offset = ScrollSafeZone.OffsetOutOfBounds(gs.Cursor);
        Rectangle nvp = Viewport.Shifted(offset);
        ScrollValidMask = ScrollValidMask.Intersect(nvp);

        Rectangle oldArea = MapToRender(ScrollValidMask);
        Viewport.Shift(offset);
        if (oldArea.Area > 0) Renderer.CopyArea(oldArea, MapToRender(ScrollValidMask));

        Viewport.ForAll(p => { if (!ScrollValidMask.Contains(p)) RenderViewPortCell(p, gs); });
        RenderBorder(gs);

        ScrollValidMask = Viewport.Clone();

        return true;
    }

    private void RenderViewPortCell(Point p, BoardState gs)
    {
        if (gs.Board.Contains(p)) RenderCell(p, gs);
        else if (gs.Board.Grow(1).Contains(p)) RenderBorderCell(p, gs);
        else ClearCell(p);
    }

    private void RenderBorderCell(Point p, BoardState gs)
    {
        StyleData data = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        // Corners
        if (p.Equals(new Point(-1,-1))) 
            MappedPrint(p.X, p.Y, data, settings.Texts["border-corner-tl"]);
        else if (p.Equals(new Point(gs.BoardWidth, -1))) 
            MappedPrint(p.X, p.Y, data, settings.Texts["border-corner-tr"]);
        else if (p.Equals(new Point(-1, gs.BoardHeight)))
            MappedPrint(p.X, p.Y, data, settings.Texts["border-corner-bl"]);
        else if (p.Equals(new Point(gs.BoardWidth, gs.BoardHeight)))
            MappedPrint(p.X, p.Y, data, settings.Texts["border-corner-br"]);

        // Edges
        else if (p.Y == -1 || p.Y == gs.BoardHeight)
            MappedPrint(p.X, p.Y, data, settings.Texts["border-horizontal"]);
        else if (p.X == -1 || p.X == gs.BoardWidth)
            MappedPrint(p.X, p.Y, data, settings.Texts["border-vertical"]);
    }

    private void ClearCell(Point p) => MappedPrint(p, hideStyle, "  ");

    private Rectangle MapToRender(Rectangle r) => new(MapToRender(r.TopLeft), MapToRender(r.BottomRight));
    private Point MapToRender(Point p) => new(OffsetX + p.X * ScaleX, OffsetY + p.Y * ScaleY);

    bool UpdateOffsets(BoardState currentGS)
    {
        if (currentGS == null) return true;
        if (Viewport == null) Viewport = currentGS.Board.Clone();

        ScaleX = settings.Dimensions["cell-size-x"];
        ScaleY = settings.Dimensions["cell-size-y"];

        int barheight = 1 + 2 * settings.Dimensions["stat-padding-y"];

        Rectangle consoleBounds = Renderer.Bounds; // Whole Console
        Rectangle newRenderMask = consoleBounds.Shrink(0, barheight, 0, 0); // Area that the board can be drawn into

        // Return if the measurements did not change
        if (RenderMask is Rectangle r && r.Equals(newRenderMask)) return false;

        // Reset render shortcuts
        RenderMask = newRenderMask;
        ScrollValidMask = Rectangle.Zero;

        // Create a new viewport to fit
        Rectangle newVP = this.Viewport.Clone();
        newVP.Width = RenderMask.Width / ScaleX;
        newVP.Height = RenderMask.Height / ScaleY;

        // Align the new viewport as best as we can
        if (Viewport.Equals(Rectangle.Zero))
            newVP.CenterOn(currentGS.Board.Center);
        else
            newVP.CenterOn(Viewport.Center);

        Viewport = newVP;

        return true;
    }

    void RenderFullBoard(BoardState currentGS)
    {
        TryCenterViewPort(currentGS);
        // Border
        Renderer.ClearScreen(hideStyle);
        RenderBorder(currentGS);

        // Tiles
        Viewport.Intersect(currentGS.Board).ForAll((x, y) => RenderCell(new Point(x, y), currentGS));
        Renderer.HideCursor(hideStyle);
        ScrollValidMask = Viewport.Clone();
    }

    private void TryCenterViewPort(BoardState currentGS)
    {
        if (currentGS.BoardWidth < ScrollSafeZone.Width && currentGS.BoardHeight < ScrollSafeZone.Height)
        {
            Viewport.CenterOn(currentGS.Board.Center);
        }
    }

    void MappedPrint(Point p, StyleData data, string s)
    { 
        if (Viewport.Contains(p))
            Renderer.PrintAtTile(MapToRender(p), data, s); 
    }
    void MappedPrint(int x, int y, StyleData data, string s) => MappedPrint(new Point(x, y), data, s);

    void RenderBorder(BoardState currentGS)
    {
        StyleData data = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        // Top
        MappedPrint(-1,-1, data, settings.Texts["border-corner-tl"]);
        for (int x = 0; x < currentGS.BoardWidth; x++) MappedPrint(x, -1, data, settings.Texts["border-horizontal"]);
        MappedPrint(currentGS.BoardWidth, -1, data, settings.Texts["border-corner-tr"]);

        // Sides
        for (int y = 0; y < currentGS.BoardHeight; y++)
        {
            MappedPrint(-1, y, data, settings.Texts["border-vertical"]);
            MappedPrint(currentGS.BoardWidth, y, data, settings.Texts["border-vertical"]);
        }

        // Bottom
        MappedPrint(-1, currentGS.BoardHeight, data, settings.Texts["border-corner-bl"]);
        for (int x = 0; x < currentGS.BoardWidth; x++) MappedPrint(x, currentGS.BoardHeight, data, settings.Texts["border-horizontal"]);
        MappedPrint(currentGS.BoardWidth, currentGS.BoardHeight, data, settings.Texts["border-corner-br"]);
    }

    void RenderCell(Point cl, BoardState currentGS)
    {
        ConsoleColor fg = GetTileVisual(cl, currentGS) switch
        {
            TileVisual.Discovered => settings.Colors["cell-fg-discovered"],
            TileVisual.Undiscovered => settings.Colors["cell-fg-undiscovered"],
            TileVisual.UndiscoveredGrid => settings.Colors["cell-fg-undiscovered-grid"],
            TileVisual.Flagged => settings.Colors["cell-flagged"],
            TileVisual.DiscoveredMine => settings.Colors["cell-mine-discovered"],
            TileVisual.DeadWrongFlag => settings.Colors["cell-dead-wrong-flag"],
            TileVisual.DeadMine => settings.Colors["cell-dead-mine-missed"],
            TileVisual.DeadMineExploded => settings.Colors["cell-dead-mine-hit"],
            TileVisual.DeadMineFlagged => settings.Colors["cell-dead-mine-flagged"],
            TileVisual.DeadDiscovered => settings.Colors["cell-fg-discovered"],
            TileVisual.DeadUndiscovered => settings.Colors["cell-fg-undiscovered"],
            TileVisual.QuestionMarked => settings.Colors["cell-questionmarked"],
            _ => settings.Colors["cell-fg-out-of-bounds"],
        };

        string text = settings.Texts["cell-empty"];
        switch (GetTileVisual(cl, currentGS))
        {
            case TileVisual.Undiscovered:
            case TileVisual.DeadUndiscovered:
            case TileVisual.UndiscoveredGrid:
                text = settings.Texts["cell-undiscovered"];
                break;

            case TileVisual.DeadWrongFlag:
            case TileVisual.DeadMineFlagged:
            case TileVisual.Flagged:
                text = settings.Texts["cell-flag"];
                break;

            case TileVisual.DeadMine:
            case TileVisual.DiscoveredMine:
            case TileVisual.DeadMineExploded:
                text = settings.Texts["cell-mine"];
                break;

            case TileVisual.QuestionMarked:
                text = settings.Texts["cell-questionmarked"];
                break;

            case TileVisual.DeadDiscovered:
            case TileVisual.Discovered:
                int num = currentGS.CellMineNumber(cl);
                if (currentGS.Difficulty.SubtractFlags) num = currentGS.CellSubtractedMineNumber(cl);
                if (num > 0 && (currentGS.Cursor.Equals(cl) || !currentGS.Difficulty.OnlyShowAtCursor))
                {
                    text = num.ToString();
                    fg = settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
                }
                else
                {
                    text = settings.Texts["cell-empty"];
                }
                break;
        }

        // Cursor
        if (currentGS.PlayerState != PlayerState.Dead && currentGS.Cursor.Equals(cl))
        {
            if (!currentGS.Difficulty.OnlyShowAtCursor || GetTileVisual(cl, currentGS) != TileVisual.Discovered || currentGS.CellMineNumber(cl) <= 0)
                fg = settings.Colors["cell-selected"];
            if (text == settings.Texts["cell-undiscovered"] || text == settings.Texts["cell-empty"])
                text = settings.Texts["cursor"];
        }

        ConsoleColor bg = GetTileVisual(cl, currentGS) switch
        {
            TileVisual.Discovered or
            TileVisual.DeadDiscovered or
            TileVisual.DeadMineExploded or
            TileVisual.DiscoveredMine => settings.Colors["cell-bg-discovered"],

            TileVisual.UndiscoveredGrid => settings.Colors["cell-bg-undiscovered-grid"],

            _ => settings.Colors["cell-bg-undiscovered"]
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

    private TileVisual GetTileVisual(Point cl, BoardState currentGS)
    {
        if (currentGS.PlayerState == PlayerState.Dead)
        {
            if (currentGS.CellIsMine(cl))
            {
                if (currentGS.CellIsFlagged(cl)) return TileVisual.DeadMineFlagged;
                if (currentGS.CellIsDiscovered(cl)) return TileVisual.DeadMineExploded;
                else return TileVisual.DeadMine;
            }
            else
            {
                if (currentGS.CellIsFlagged(cl)) return TileVisual.DeadWrongFlag;
                if (currentGS.CellIsDiscovered(cl)) return TileVisual.DeadDiscovered;
                else return TileVisual.DeadUndiscovered;
            }
        }
        else
        {
            if (currentGS.CellIsDiscovered(cl) && currentGS.CellIsMine(cl)) return TileVisual.DiscoveredMine;
            if (currentGS.CellIsDiscovered(cl)) return TileVisual.Discovered;
            if (currentGS.CellIsFlagged(cl)) return TileVisual.Flagged;
            if (currentGS.CellIsQuestionMarked(cl)) return TileVisual.QuestionMarked;
            if ((cl.X) % settings.Dimensions["cell-grid-size"] == 0) return TileVisual.UndiscoveredGrid;
            if ((cl.Y) % settings.Dimensions["cell-grid-size"] == 0) return TileVisual.UndiscoveredGrid;
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
