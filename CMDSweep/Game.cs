using System;
using System.Timers;

namespace CMDSweep
{
    class Game
    {
        enum GameRenderState
        {
            Playing,
            Menu,
            Dead,
            Highscore,
        }

        GameRenderState grs;

        IRenderer renderer;
        Timer t;
        Bounds bounds;
        BoardVisualizer bv;
        GameState currentState;

        public Game(IRenderer r)
        {
            renderer = r;
            bounds = r.Bounds;
            InitialiseGame();
            bv = new BoardVisualizer(r);

            t = new Timer(200);

            t.Elapsed += TimerElapsed;
            t.AutoReset = true;
            t.Start();
            Refresh(false);

        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) => Refresh(false);
        
        private void Refresh(bool force)
        {
            if (bounds != renderer.Bounds)
            {
                force = true;
                bounds = renderer.Bounds;
                Console.WriteLine("Change detected");
            }

            switch (grs)
            {
                case GameRenderState.Playing:
                    bv.Visualize(currentState, force);

                    currentState = currentState.Clone();

                    if (currentState.CellIsFlagged(0, 0))
                        currentState.Unflag(0, 0);
                    else
                        currentState.Flag(0,0);

                    break;
                default:
                    break;
            }

        }

        public void InitialiseGame()
        {
            grs = GameRenderState.Playing;
            CellData[,] cd = new CellData[1, 1];
            cd[0, 0] = new CellData(true, false, false);
            currentState = new GameState(cd);
        }
    }
}
