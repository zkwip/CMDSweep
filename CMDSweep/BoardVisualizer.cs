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
        Rectangle RenderMask; // the area the board can be drawn into (screen space
        Rectangle Viewport; // rendermask mapped to board space

        private GameBoardState? lastRenderedGameState;
        private readonly StyleData hideStyle;


        public BoardVisualizer(GameApp g)
        {
            renderer = g.Renderer;
            settings = g.Settings;
            game = g;
            hideStyle = new StyleData(settings.Colors["cell-bg-out-of-bounds"], settings.Colors["cell-bg-out-of-bounds"], false);
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

            // Mutex for the renderer
            if (rendering) return false;
            rendering = true;

            // Actual render loop
            while (modeWaiting != RefreshMode.None)
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
                    if (ScrollBoard(curGS)) mode = RefreshMode.Scroll; // Scrolling
                    if (curGS.PlayerState != prevGS.PlayerState) mode = RefreshMode.Full; // Player Mode changed
                }

                //Render
                if (mode == RefreshMode.ChangesOnly)
                    RenderBoardChanges(curGS, prevGS!);
                else
                    RenderFullBoard(curGS);

                RenderStatBoard(curGS);
                renderer.HideCursor(hideStyle);
                lastRenderedGameState = curGS;
            }

            rendering = false;
            return true;
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
            StyleData clockStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
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

            StyleData faceStyle = new StyleData(settings.Colors["face-fg"], settings.Colors["face-bg"]);
            renderer.PrintAtTile(p, faceStyle, face);
        }

        private void RenderMineCounter(Point p, GameBoardState currentGS)
        {
            StyleData minesLeftStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
            renderer.PrintAtTile(p, minesLeftStyle, string.Format(" {0:D3} ", currentGS.MinesLeft));
        }

        private void RenderLifeCounter(Point p, GameBoardState currentGS)
        {
            char life = settings.Texts["stat-life"][0];
            StyleData livesLeftStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
            StyleData livesGoneStyle = new StyleData(settings.Colors["stat-lives-lost"], settings.Colors["stat-mines-bg"]);

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
            // Check if the cursor is outside the scroll safe zone
            if (CursorInScrollSafezone(gs)) return false;

            Offset offset = ScrollSafeZone.OffsetOutOfBounds(gs.Cursor);
            Viewport.Shift(offset);

            ScrollValidMask = ScrollValidMask.Intersect(MapToRender(Viewport));

            return true;
        }

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
            ScrollBoard(currentGS);

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
            ScrollValidMask = RenderMask.Clone();
        }

        void MappedPrint(Point p, StyleData data, string s) => renderer.PrintAtTile(MapToRender(p), data, s);
        void MappedPrint(int x, int y, StyleData data, string s) => MappedPrint(new Point(x, y), data, s);

        void RenderBorder(GameBoardState currentGS)
        {
            StyleData data = new StyleData(settings.Colors["border-fg"], settings.Colors["cell-bg-out-of-bounds"], false);

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
