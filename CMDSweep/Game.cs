using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Web;

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
        Settings settings;

        IRenderer renderer;
        Timer t;
        Bounds bounds;
        BoardVisualizer bv;
        GameState currentState;

        public Game(IRenderer r)
        {
            LoadSettings();
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

        public class Settings
        {
            public List<Difficulty> Difficulties;
            public Dictionary<string, string> Colors;
            public Dictionary<string, char> Symbols;
            public Dictionary<string, int> Dimensions;
        }

        public class Difficulty
        {
            public string Name;
            public int Width;
            public int Height;
            public int Mines;
        }

        private void LoadSettings()
        {
            using (StreamReader r = new StreamReader("../../settings.json"))
            {
                string json = r.ReadToEnd();
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
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
                    break;
                default:
                    break;
            }

        }

        public void InitialiseGame()
        {
            grs = GameRenderState.Playing;
            currentState = GameState.NewGame(10,10,10,4,4,2);
        }
    }
}
