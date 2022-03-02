using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class BoardVisualizer
    {
        readonly IRenderer renderer;
        readonly GameApp game;
        readonly GameSettings settings;

        private int offsetX => RenderMask.Left - Viewport.Left * scaleX;
        private int offsetY => RenderMask.Top - Viewport.Top * scaleY;

        private int scaleX = 1;
        private int scaleY = 1;

        private bool rendering = false;
        private RefreshMode modeWaiting = RefreshMode.None;

        Rectangle ScrollValidMask; // the area that is still valid (board space)
        Rectangle RenderMask; // the area the board can be drawn into (screen space)
        Rectangle Viewport; // rendermask mapped to (board space)

        private GameBoardState? lastRenderedGameState;
        private readonly StyleData hideStyle;


        public BoardVisualizer(GameApp g)
        {
            renderer = g.Renderer;
            settings = g.Settings;
            game = g;
            hideStyle = settings.GetStyle("cell-bg-out-of-bounds", "cell-bg-out-of-bounds");
            RenderMask = Rectangle.Zero;
            Viewport = Rectangle.Zero;
            ScrollValidMask = Rectangle.Zero;
        }

        private void SetVisQueue(RefreshMode mode)
        {
            if (mode > modeWaiting) modeWaiting = mode;
        }

        public bool Visualize(RefreshMode mode)
        {
            // Indicate there is stuff to redraw
            SetVisQueue(mode);

            // Defer renderer
            if (rendering) return false;
            rendering = true;

            // Actual render loop
            while (modeWaiting != RefreshMode.None)
            {
                lock (renderer)
                {
                    // Reset queue
                    mode = modeWaiting;
                    modeWaiting = RefreshMode.None;

                    GameBoardState? prevGS = lastRenderedGameState;
                    GameBoardState? curGS = game.CurrentState;

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
                    if (mode == RefreshMode.Full)
                    {
                        RenderFullBoard(curGS);
                    }
                    else
                    {
                        if (mode == RefreshMode.Scroll)
                            ScrollBoard(curGS);

                        RenderBoardChanges(curGS, prevGS!);
                    }

                    RenderStatBoard(curGS);

                    if (curGS.PlayerState == PlayerState.Win) RenderPopup("You won! \n \n Test");
                    if (curGS.PlayerState == PlayerState.Dead) RenderPopup("You died! \n \n Test");

                    renderer.HideCursor(hideStyle);
                    lastRenderedGameState = curGS;
                }
            }

            rendering = false;
            return true;
        }

        private void RenderPopup(string text)
        {
            int xpad = settings.Dimensions["popup-padding-x"];
            int ypad = settings.Dimensions["popup-padding-y"];
            StyleData style = settings.GetStyle("popup");

            int horRoom = renderer.Bounds.Width - (4 * xpad);
            int verRoom = renderer.Bounds.Height - (4 * ypad);

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
                    lines.Insert(i, line.Substring(0, breakpoint));
                    lines.Insert(i + 1, line.Substring(0, breakpoint + 1));
                }

                broadest = Math.Max(broadest, lines[i].Length);
            }

            Rectangle textbox = new Rectangle(0, 0, broadest, lines.Count);
            textbox.CenterOn(renderer.Bounds.Center);

            RenderPopupBox(style, textbox.Grow(xpad, ypad, xpad, ypad), "popup-border");

            for (int i = 0; i < lines.Count; i++)
            {
                renderer.PrintAtTile(textbox.TopLeft.Shifted(0, i), style, lines[i]);
            }

        }

        private void RenderPopupBox(StyleData style, Rectangle r, string border)
        {
            renderer.ClearScreen(style, r);

            r = r.Shrink(0,0,1,1); // since it is exclusive
            r.HorizontalRange.ForEach((i) => renderer.PrintAtTile(new(i,r.Top), style, settings.Texts[border + "-side-top"]));
            r.HorizontalRange.ForEach((i) => renderer.PrintAtTile(new(i, r.Bottom), style, settings.Texts[border + "-side-bottom"]));

            r.VerticalRange.ForEach((i) => renderer.PrintAtTile(new(r.Left, i), style, settings.Texts[border + "-side-left"]));
            r.VerticalRange.ForEach((i) => renderer.PrintAtTile(new(r.Right, i), style, settings.Texts[border + "-side-right"]));

            renderer.PrintAtTile(r.TopLeft, style, settings.Texts[border + "-corner-tl"]);
            renderer.PrintAtTile(r.BottomLeft, style, settings.Texts[border + "-corner-bl"]);
            renderer.PrintAtTile(r.TopRight, style, settings.Texts[border + "-corner-tr"]);
            renderer.PrintAtTile(r.BottomRight, style, settings.Texts[border + "-corner-br"]);

        }

        private void RenderBoardChanges(GameBoardState curGS, GameBoardState prevGS)
        {
            List<Point> changes;
            changes = curGS.CompareForChanges(prevGS, Viewport);
            foreach (Point cl in changes) RenderCell(cl, curGS);
        }

        private void RenderStatBoard(GameBoardState currentGS)
        {
            TableGrid bar = new();
            bar.Bounds = new(renderer.Bounds.HorizontalRange, LinearRange.Zero);

            int vpad = settings.Dimensions["stat-padding-x"];
            int hpad = settings.Dimensions["stat-padding-y"];

            // Rows
            bar.AddRow(hpad, 0);
            bar.AddRow(1, 0, "bar");
            bar.AddRow(hpad, 0);

            // Columns
            bar.AddColumn(vpad, 0);
            bar.AddColumn(6, 0, "clock");
            bar.AddColumn(vpad, 1);
            bar.AddColumn(4, 0, "face");
            bar.AddColumn(vpad, 1);
            bar.AddColumn(5, 0, "lives");
            bar.AddColumn(settings.Dimensions["stat-padding-x-in-between"], 0);
            bar.AddColumn(6, 0, "mines");
            bar.AddColumn(vpad, 0);

            renderer.ClearScreen(hideStyle, bar.Bounds);

            RenderClock(bar.GetPoint("clock","bar"), currentGS);
            RenderFace(bar.GetPoint("face", "bar"), currentGS);
            RenderLifeCounter(bar.GetPoint("lives", "bar"), currentGS);
            RenderMineCounter(bar.GetPoint("mines", "bar"), currentGS);

        }

        private void RenderClock(Point p, GameBoardState currentGS)
        {
            StyleData clockStyle = settings.GetStyle("stat-mines");
            renderer.PrintAtTile(p, clockStyle, currentGS.Time.ToString(@"\ h\:mm\:ss\ "));
        }

        private void RenderFace(Point p, GameBoardState currentGS)
        {
            string face = ":)";
            switch (currentGS.Face)
            {
                default:
                case Face.Normal:
                    face = settings.Texts["face-normal"]; break;
                case Face.Surprise:
                    face = settings.Texts["face-surprise"];break;
                case Face.Win:
                    face = settings.Texts["face-win"]; break;
                case Face.Dead:
                    face = settings.Texts["face-dead"]; break;
            }

            StyleData faceStyle = settings.GetStyle("face");
            renderer.PrintAtTile(p, faceStyle, face);
        }

        private void RenderMineCounter(Point p, GameBoardState currentGS)
        {
            StyleData minesLeftStyle = settings.GetStyle("stat-mines");
            renderer.PrintAtTile(p, minesLeftStyle, string.Format(" {0:D3} ", currentGS.MinesLeft));
        }

        private void RenderLifeCounter(Point p, GameBoardState currentGS)
        {
            char life = settings.Texts["stat-life"][0];
            StyleData livesLeftStyle = settings.GetStyle("stat-mines");
            StyleData livesGoneStyle = settings.GetStyle("stat-lives-lost","stat-mines-bg");

            string atext = " ";
            for (int i = 0; i < currentGS.Difficulty.Lives - currentGS.LivesLost; i++) atext += life + " ";

            string btext = "";
            for (int i = 0; i < currentGS.LivesLost; i++) btext += life + " ";

            renderer.PrintAtTile(p, livesLeftStyle, atext);
            renderer.PrintAtTile(p.Shifted(atext.Length, 0), livesGoneStyle, btext);
        }
        Rectangle ScrollSafeZone => Viewport.Shrink(settings.Dimensions["scroll-safezone"]);
        bool CursorInScrollSafezone(GameBoardState gs) => ScrollSafeZone.Contains(gs.Cursor);
        bool ScrollBoard(GameBoardState gs)
        {
            // Change the offset
            Offset offset = ScrollSafeZone.OffsetOutOfBounds(gs.Cursor);
            Rectangle nvp = Viewport.Shifted(offset);
            ScrollValidMask = ScrollValidMask.Intersect(nvp);

            Rectangle oldArea = MapToRender(ScrollValidMask);
            Viewport.Shift(offset);
            if (oldArea.Area > 0) renderer.CopyArea(oldArea, MapToRender(ScrollValidMask));

            Viewport.ForAll(p => { if (!ScrollValidMask.Contains(p)) RenderViewPortCell(p, gs); });
            RenderBorder(gs);

            ScrollValidMask = Viewport.Clone();

            return true;
        }

        private void RenderViewPortCell(Point p, GameBoardState gs)
        {
            if (gs.Board.Contains(p)) RenderCell(p, gs);
            else if (gs.Board.Grow(1).Contains(p)) RenderBorderCell(p, gs);
            else ClearCell(p);
        }

        private void RenderBorderCell(Point p, GameBoardState gs)
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
        private Point MapToRender(Point p) => new(offsetX + p.X * scaleX, offsetY + p.Y * scaleY);

        bool UpdateOffsets(GameBoardState currentGS)
        {
            if (currentGS == null) return true;
            if (Viewport == null) Viewport = currentGS.Board.Clone();

            scaleX = settings.Dimensions["cell-size-x"];
            scaleY = settings.Dimensions["cell-size-y"];

            int barheight = 1 + 2 * settings.Dimensions["stat-padding-y"];

            Rectangle consoleBounds = renderer.Bounds; // Whole Console
            Rectangle newRenderMask = consoleBounds.Shrink(0, barheight, 0, 0); // Area that the board can be drawn into

            // Return if the measurements did not change
            if (RenderMask is Rectangle r && r.Equals(newRenderMask)) return false;

            // Reset render shortcuts
            RenderMask = newRenderMask;
            ScrollValidMask = Rectangle.Zero;

            // Create a new viewport to fit
            Rectangle newVP = this.Viewport.Clone();
            newVP.Width = RenderMask.Width / scaleX;
            newVP.Height = RenderMask.Height / scaleY;

            // Align the new viewport as best as we can
            if (Viewport.Equals(Rectangle.Zero))
                newVP.CenterOn(currentGS.Board.Center);
            else
                newVP.CenterOn(Viewport.Center);

            Viewport = newVP;

            return true;
        }

        void RenderFullBoard(GameBoardState currentGS)
        {
            // Border
            renderer.ClearScreen(hideStyle);
            RenderBorder(currentGS);

            // Tiles
            Viewport.Intersect(currentGS.Board).ForAll((x, y) => RenderCell(new Point(x, y), currentGS));
            renderer.HideCursor(hideStyle);
            ScrollValidMask = Viewport.Clone();
        }

        void MappedPrint(Point p, StyleData data, string s)
        { 
            if (Viewport.Contains(p))
                renderer.PrintAtTile(MapToRender(p), data, s); 
        }
        void MappedPrint(int x, int y, StyleData data, string s) => MappedPrint(new Point(x, y), data, s);

        void RenderBorder(GameBoardState currentGS)
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

        void RenderCell(Point cl, GameBoardState currentGS)
        {

            ConsoleColor fg;
            switch (GetTileVisual(cl, currentGS))
            {
                case TileVisual.Discovered: fg = settings.Colors["cell-fg-discovered"]; break;
                case TileVisual.Undiscovered: fg = settings.Colors["cell-fg-undiscovered"]; break;
                case TileVisual.UndiscoveredGrid: fg = settings.Colors["cell-fg-undiscovered-grid"]; break;
                case TileVisual.Flagged: fg = settings.Colors["cell-flagged"]; break;
                case TileVisual.DiscoveredMine: fg = settings.Colors["cell-mine-discovered"]; break;

                case TileVisual.DeadWrongFlag: fg = settings.Colors["cell-dead-wrong-flag"]; break;
                case TileVisual.DeadMine: fg = settings.Colors["cell-dead-mine-missed"]; break;
                case TileVisual.DeadMineExploded: fg = settings.Colors["cell-dead-mine-hit"]; break;
                case TileVisual.DeadMineFlagged: fg = settings.Colors["cell-dead-mine-flagged"]; break;
                case TileVisual.DeadDiscovered: fg = settings.Colors["cell-fg-discovered"]; break;
                case TileVisual.DeadUndiscovered: fg = settings.Colors["cell-fg-undiscovered"]; break;
                case TileVisual.QuestionMarked: fg = settings.Colors["cell-questionmarked"]; break;
                default: fg = settings.Colors["cell-fg-out-of-bounds"]; break;
            }

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

            ConsoleColor bg;
            switch (GetTileVisual(cl, currentGS))
            {
                case TileVisual.Discovered:
                case TileVisual.DeadDiscovered:
                case TileVisual.DeadMineExploded:
                case TileVisual.DiscoveredMine:
                    //case TileVisual.DeadMine:
                    bg = settings.Colors["cell-bg-discovered"];
                    break;

                case TileVisual.UndiscoveredGrid:
                    bg = settings.Colors["cell-bg-undiscovered-grid"];
                    break;

                default:
                    bg = settings.Colors["cell-bg-undiscovered"];
                    break;
            }

            StyleData data = new StyleData(fg, bg, false);

            // Padding
            int padRight = 0;
            if (text.Length < scaleX) padRight = scaleX - text.Length;
            int padLeft = padRight / 2;
            padRight = scaleX - padLeft;

            // Actual rendering
            MappedPrint(cl, data, " ".PadLeft(padLeft));
            MappedPrint(cl, data, text.PadRight(padRight));
        }

        private TileVisual GetTileVisual(Point cl, GameBoardState currentGS)
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
}
