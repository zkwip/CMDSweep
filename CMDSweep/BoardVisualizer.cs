using System;
using System.Collections.Generic;

namespace CMDSweep
{
    class BoardVisualizer
    {
        readonly IRenderer renderer;
        public BoardVisualizer(IRenderer r)
        {
            renderer = r;
            CalculateDimensions();
        }

        GameState currentGS;
        
        public bool Visualize(GameState gs, bool full)
        {
            full = (currentGS == null || full);
            List<CellLocation> changes;

            if (full)
            {
                RenderFullBoard();
            }
            else
            {
                changes = gs.CompareForChanges(currentGS);
                Console.WriteLine("Rendering Partial board ({0})", changes.Count);
                if (changes.Count == 0) return false;
                foreach (CellLocation cl in changes) RenderAtLocation(cl.X, cl.Y);
            }

            currentGS = gs;
            
            return true;
        }

        void CalculateDimensions()
        {

        }

        void RenderFullBoard()
        {
            CalculateDimensions();
        }
        
        void RenderAtLocation(int x, int y)
        {
            Console.WriteLine("Rendering at ({0},{1})",x,y);
        }

    }
}
