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
                currentGS = gs;
                RenderFullBoard();
            }
            else
            {
                changes = gs.CompareForChanges(currentGS);
                currentGS = gs;
                Console.WriteLine("Rendering Partial board ({0})", changes.Count);
                if (changes.Count == 0) return false;
                foreach (CellLocation cl in changes) RenderAtLocation(cl.X, cl.Y);
            }
            
            return true;
        }

        void CalculateDimensions()
        {

        }

        void RenderFullBoard()
        {
            Console.WriteLine("Rendering full board at of size ({0} x {1})", currentGS.BoardWidth, currentGS.BoardHeight);
            CalculateDimensions();
        }
        
        void RenderAtLocation(int x, int y)
        {
            Console.WriteLine("Rendering at ({0},{1})",x,y);
        }

    }
}
