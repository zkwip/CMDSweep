using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class BoardVisualizer
    {
        readonly IRenderer renderer;
        readonly Game game;
        readonly GameSettings settings;

        private int offsetX = 0;
        private int offsetY = 0;
        private int scaleX = 1;
        private int scaleY = 1;
        private bool rendering = false;
        private bool renderWaiting = false;
        private bool active = true;

        private GameState lastRenderedGameState;
        private readonly StyleData hideStyle;
        private RefreshMode lastRefresh = RefreshMode.Rescale;

        public BoardVisualizer(Game g)
        {
            renderer = g.Renderer;
            settings = g.Settings;
            game = g;
            UpdateDimensions();
            hideStyle = new StyleData(settings.Colors["cell-bg-out-of-bounds"], settings.Colors["cell-bg-out-of-bounds"], false); 
        }

        GameState CurrentState { get => game.CurrentState; }

        public bool Visualize(RefreshMode mode)
        {
            // Indicate there is stuff to redraw
            renderWaiting = true;

            // Mutex for the renderer
            if (rendering) return false;

            rendering = true;

            // Actual render loop
            while (renderWaiting)
            {
                renderWaiting = false;
                ProcessVisualization(mode);
            }

            rendering = false;
            return true;
        }
        
        public void ProcessVisualization(RefreshMode mode)
        {
            // Force a full rerender in case the screen has not been drawn before
            if ((lastRenderedGameState == null) || 
                (mode == RefreshMode.ChangesOnly && CurrentState.PlayerState != lastRenderedGameState.PlayerState) || 
                (lastRefresh == RefreshMode.Rescale))
            {
                mode = RefreshMode.Full;
            }


            List<CellLocation> changes; 

            if (mode != RefreshMode.ChangesOnly)
            {
                lastRenderedGameState = CurrentState;
                RenderFullBoard();
            }
            else
            {
                changes = CurrentState.CompareForChanges(lastRenderedGameState);
                lastRenderedGameState = CurrentState;
                foreach (CellLocation cl in changes) RenderAtLocation(cl);
            }

            UpdateStatBoard();
            renderer.HideCursor(hideStyle);
            lastRefresh = mode;
        }

        private void UpdateStatBoard()
        {

            int left = settings.Dimensions["stat-padding-x"];
            int center = renderer.Bounds.Width / 2;
            int right = renderer.Bounds.Width - settings.Dimensions["stat-padding-x"];

            int top = settings.Dimensions["stat-padding-y"];

            int minesStart = right - 5;
            int livesStart = minesStart - (2*CurrentState.Difficulty.Lives) - 1 - settings.Dimensions["stat-padding-x-in-between"];

            renderer.ClearScreen(hideStyle, top);

            RenderTimeCounter(top, left);
            RenderFace(top, center - 1);
            RenderLifeCounter(top, livesStart);
            RenderMineCounter(top, minesStart);

        }

        private void RenderTimeCounter(int row, int col)
        {
            StyleData clockStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
            renderer.PrintAtTile(row, col, clockStyle, CurrentState.Time.ToString(@"\ h\:mm\:ss\ "));
        }

        private void RenderFace(int row, int col)
        {
            string face = ":)";
            switch (CurrentState.Face)
            {
                default:
                case Face.Normal:
                    face = settings.Texts["face-normal"]; break;
                case Face.Surprise:
                    face = settings.Texts["face-surprise"];
                    break;
                case Face.Win:
                    face = settings.Texts["face-win"]; break;
                case Face.Dead:
                    face = settings.Texts["face-dead"]; break;
            }

            StyleData faceStyle = new StyleData(settings.Colors["face-fg"], settings.Colors["face-bg"]);
            renderer.PrintAtTile(row, col, faceStyle, face);
        }

        private void RenderMineCounter(int row, int col)
        {
            StyleData minesLeftStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
            renderer.PrintAtTile(row, col, minesLeftStyle, string.Format(" {0:D3} ", CurrentState.MinesLeft));
        }

        private void RenderLifeCounter(int row, int col)
        {
            char life = settings.Texts["stat-life"][0];
            StyleData livesLeftStyle = new StyleData(settings.Colors["stat-mines-fg"], settings.Colors["stat-mines-bg"]);
            StyleData livesGoneStyle = new StyleData(settings.Colors["stat-lives-lost"], settings.Colors["stat-mines-bg"]);

            string atext = " ";
            for (int i = 0; i < CurrentState.Difficulty.Lives - CurrentState.LivesLost; i++) atext += life + " ";

            string btext = "";
            for (int i = 0; i < CurrentState.LivesLost; i++) btext += life + " ";

            renderer.PrintAtTile(row, col, livesLeftStyle, atext);
            renderer.PrintAtTile(row, col + atext.Length, livesGoneStyle, btext);
        }

        bool UpdateDimensions()
        {
            bool succes = true;

            scaleX = settings.Dimensions["cell-size-x"];
            scaleY = settings.Dimensions["cell-size-y"];

            int reqWidth = CurrentState.BoardWidth * scaleX;
            int reqHeight = CurrentState.BoardHeight * scaleY;

            offsetX = (renderer.Bounds.Width - reqWidth) / 2;
            offsetY = (renderer.Bounds.Height - reqHeight) / 2;

            if (offsetX < scaleX || offsetY < scaleY) succes = false;

            return succes;
        }

        void RenderFullBoard()
        {
            if (UpdateDimensions())
            {
                renderer.ClearScreen(hideStyle);
                RenderBorder();
                for (int y = 0; y < CurrentState.BoardHeight; y++)
                {
                    for (int x = 0; x < CurrentState.BoardWidth; x++) RenderAtLocation(new CellLocation(x, y));
                }
                renderer.HideCursor(hideStyle);
            }
            else
            {
                // TDOD: Handle a board that does not fit on screen
            }
        }

        void RenderBorder()
        {
            StyleData data = new StyleData(settings.Colors["border-fg"], settings.Colors["cell-bg-out-of-bounds"],false);

            renderer.PrintAtTile(offsetY - scaleY, offsetX - scaleX, data, settings.Texts["border-corner-tl"]);
            for (int x = 0; x < CurrentState.BoardWidth; x++)
                renderer.PrintAtTile(offsetY - scaleY, offsetX + x * scaleX, data, settings.Texts["border-horizontal"]);

            renderer.PrintAtTile(offsetY - scaleY, offsetX + CurrentState.BoardWidth * scaleX, data, settings.Texts["border-corner-tr"]);

            for (int y = 0; y < CurrentState.BoardHeight; y++)
            {
                renderer.PrintAtTile(offsetY + y * scaleY, offsetX - scaleX, data, settings.Texts["border-vertical"]);
                renderer.PrintAtTile(offsetY + y * scaleY, offsetX + CurrentState.BoardWidth * scaleX, data, settings.Texts["border-vertical"]);
            }


            renderer.PrintAtTile(offsetY + CurrentState.BoardHeight * scaleY, offsetX - scaleX, data, settings.Texts["border-corner-bl"]);
            for (int x = 0; x < CurrentState.BoardWidth; x++)
                renderer.PrintAtTile(offsetY + CurrentState.BoardHeight * scaleY, offsetX + x * scaleX, data, settings.Texts["border-horizontal"]);
            renderer.PrintAtTile(offsetY + CurrentState.BoardHeight * scaleY, offsetX + CurrentState.BoardWidth * scaleX, data, settings.Texts["border-corner-br"]);
        }
        
        void RenderAtLocation(CellLocation cl)
        {
            int posX = offsetX + cl.X * scaleX;
            int posY = offsetY + cl.Y * scaleY;
            
            ConsoleColor fg;
            switch (GetTileVisual(cl))
            {
                case TileVisual.Discovered:         fg = settings.Colors["cell-fg-discovered"];     break;
                case TileVisual.Undiscovered:       fg = settings.Colors["cell-fg-undiscovered"];   break;
                case TileVisual.Flagged:            fg = settings.Colors["cell-flagged"];           break;
                case TileVisual.DiscoveredMine:     fg = settings.Colors["cell-mine-discovered"];   break;

                case TileVisual.DeadWrongFlag:      fg = settings.Colors["cell-dead-wrong-flag"];   break;
                case TileVisual.DeadMine:           fg = settings.Colors["cell-dead-mine-missed"];  break;
                case TileVisual.DeadMineExploded:   fg = settings.Colors["cell-dead-mine-hit"];     break;
                case TileVisual.DeadMineFlagged:    fg = settings.Colors["cell-dead-mine-flagged"]; break;
                case TileVisual.DeadDiscovered:     fg = settings.Colors["cell-fg-discovered"];     break;
                case TileVisual.DeadUndiscovered:   fg = settings.Colors["cell-fg-undiscovered"];   break;
                case TileVisual.QuestionMarked:     fg = settings.Colors["cell-questionmarked"];    break;
                default:                            fg = settings.Colors["cell-fg-out-of-bounds"];  break;
            }

            string text = settings.Texts["cell-empty"];
            switch (GetTileVisual(cl))
            {
                case TileVisual.Undiscovered:
                case TileVisual.DeadUndiscovered:
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
                    int num = CurrentState.CellMineNumber(cl);
                    if (CurrentState.Difficulty.SubtractFlags) num = CurrentState.CellSubtractedMineNumber(cl);
                    if (num > 0 && (CurrentState.Cursor == cl || !CurrentState.Difficulty.OnlyShowAtCursor))
                    {
                        text = num.ToString();
                        fg = settings.Colors[string.Format("cell-{0}-discovered", num%10)];
                    }
                    else
                    {
                        text = settings.Texts["cell-empty"];
                    }
                    break;
            }

            // Cursor
            if (CurrentState.PlayerState != PlayerState.Dead && CurrentState.Cursor == cl)
            {
                if (!CurrentState.Difficulty.OnlyShowAtCursor || GetTileVisual(cl) != TileVisual.Discovered || CurrentState.CellMineNumber(cl) <= 0)
                    fg = settings.Colors["cell-selected"];
                if (text == settings.Texts["cell-undiscovered"] || text == settings.Texts["cell-empty"])
                    text = settings.Texts["cursor"];
            }

            ConsoleColor bg;
            switch(GetTileVisual(cl))
            {
                case TileVisual.Discovered:
                case TileVisual.DeadDiscovered:
                case TileVisual.DeadMineExploded:
                case TileVisual.DiscoveredMine:
                    //case TileVisual.DeadMine:
                    bg = settings.Colors["cell-bg-discovered"];
                    break;

                default:
                    bg = settings.Colors["cell-bg-undiscovered"];
                    break;
            }

            StyleData data = new StyleData(fg,bg,false);
            
            // Padding
            int padRight = 0;
            if (text.Length < scaleX) padRight = scaleX - text.Length;
            int padLeft = padRight / 2;
            padRight = scaleX-padLeft;

            // Actual rendering
            renderer.PrintAtTile(posY, posX, data, " ".PadLeft(padLeft));
            renderer.PrintAtTile(posY, posX, data, text.PadRight(padRight));
            // It goes wrong here somewhere
        }

        private TileVisual GetTileVisual(CellLocation cl)
        {
            if(CurrentState.PlayerState == PlayerState.Dead)
            {
                if (CurrentState.CellIsMine(cl))
                {
                    if (CurrentState.CellIsFlagged(cl)) return TileVisual.DeadMineFlagged;
                    if (CurrentState.CellIsDiscovered(cl)) return TileVisual.DeadMineExploded;
                    else return TileVisual.DeadMine;
                }
                else
                {
                    if (CurrentState.CellIsFlagged(cl)) return TileVisual.DeadWrongFlag;
                    if (CurrentState.CellIsDiscovered(cl)) return TileVisual.DeadDiscovered;
                    else return TileVisual.DeadUndiscovered;
                }
            }
            else
            {
                if (CurrentState.CellIsDiscovered(cl) && CurrentState.CellIsMine(cl)) return TileVisual.DiscoveredMine;
                if (CurrentState.CellIsDiscovered(cl)) return TileVisual.Discovered;
                if (CurrentState.CellIsFlagged(cl)) return TileVisual.Flagged;
                if (CurrentState.CellIsQuestionMarked(cl)) return TileVisual.QuestionMarked;
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
        }

    }
}
