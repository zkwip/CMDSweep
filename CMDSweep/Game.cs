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

        public Game(IRenderer r)
        {
            renderer = r;
            bounds = r.Bounds;
            InitialiseGame();

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
            }

            switch (grs)
            {
                case GameRenderState.Playing:
                    RenderGame(force);
                    break;
                default:
                    break;
            }

        }

        private void RenderGame(bool force)
        {
            throw new NotImplementedException();
        }

        public void InitialiseGame()
        {
            grs = GameRenderState.Playing;
        }
    }
}
