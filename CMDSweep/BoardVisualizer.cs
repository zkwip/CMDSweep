using System;
using System.Collections.Generic;

namespace CMDSweep
{
    class BoardVisualizer
    {
        readonly IRenderer renderer;
        readonly Game game;
        readonly GameSettings settings;

        private int offsetX = 0;
        private int offsetY = 0;
        private int scaleX = 1;
        private int scaleY = 1;

        private GameState lastRenderedGameState;
        private StyleData styleOutOfBounds;
        private RefreshMode lastRefresh = RefreshMode.Rescale;

        public BoardVisualizer(Game g)
        {
            renderer = g.Renderer;
            settings = g.Settings;
            game = g;
            UpdateDimensions();
            styleOutOfBounds = new StyleData(settings.Colors["cell-fg-out-of-bounds"], settings.Colors["cell-bg-out-of-bounds"], false); 
        }

        GameState CurrentState { get => game.CurrentState; }

        
        public bool Visualize(RefreshMode mode)
        {
            if(lastRenderedGameState == null && mode == RefreshMode.ChangesOnly) mode = RefreshMode.Full;
            List<CellLocation> changes;

            if (mode != RefreshMode.ChangesOnly || lastRefresh == RefreshMode.Rescale)
            {
                lastRenderedGameState = CurrentState;
                RenderFullBoard();
            }
            else
            {
                changes = CurrentState.CompareForChanges(lastRenderedGameState);

                lastRenderedGameState = CurrentState;
                foreach (CellLocation cl in changes) RenderAtLocation(cl.X, cl.Y);
            }
            UpdateStatBoard();
            lastRefresh = mode;
            return true;
        }

        private void UpdateStatBoard()
        {
            renderer.HideCursor(styleOutOfBounds);
            int minesleft = CurrentState.MinesLeft;
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

            if (offsetX <= 0 || offsetY <= 0) succes = false;

            return succes;
        }

        void RenderFullBoard()
        {
            if (UpdateDimensions())
            {
                renderer.ClearScreen(styleOutOfBounds);

                for (int y = 0; y < CurrentState.BoardHeight; y++)
                {
                    for (int x = 0; x < CurrentState.BoardWidth; x++) RenderAtLocation(x, y);
                }
            }
            else
            {
                // TDOD: Handle a board that does not fit on screen
            }
        }
        
        void RenderAtLocation(int x, int y)
        {
            int posX = offsetX + x * scaleX;
            int posY = offsetY + y * scaleY;

            string text = "x ";

            ConsoleColor fg = settings.Colors["cell-fg-undiscovered"];
            ConsoleColor bg = settings.Colors["cell-bg-undiscovered"];

            StyleData data = new StyleData(fg,bg,false);

            renderer.PrintAtTile(posY, posX, data, text);
        }

    }
}
