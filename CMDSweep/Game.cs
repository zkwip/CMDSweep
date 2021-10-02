using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

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

        Timer refreshTimer;
        Bounds screenBounds;

        public readonly GameSettings Settings;
        public readonly IRenderer Renderer;
        public readonly BoardVisualizer Visualizer;

        public GameState CurrentState;
        public Difficulty CurrentDifficulty;

        public Game(IRenderer r)
        {
            Settings = LoadSettings();
            Renderer = r;
            screenBounds = r.Bounds;

            InitialiseGame();
            Visualizer = new BoardVisualizer(this);


            refreshTimer = new Timer(200);
            refreshTimer.Elapsed += TimerElapsed;
            refreshTimer.AutoReset = true;
            refreshTimer.Start();

            Refresh(RefreshMode.Rescale);
        }

        private GameSettings LoadSettings()
        {
            using (StreamReader r = new StreamReader("../../settings.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<GameSettings>(json);
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) => Refresh(RefreshMode.ChangesOnly);
        
        private void Refresh(RefreshMode mode)
        {
            if (screenBounds != Renderer.Bounds)
            {
                mode = RefreshMode.Rescale;
                screenBounds = Renderer.Bounds;
                Console.WriteLine("Change detected");
            }

            switch (grs)
            {
                case GameRenderState.Playing:
                    Visualizer.Visualize(mode);
                    CurrentState = CurrentState.Clone();
                    break;
                default:
                    break;
            }

        }

        public void InitialiseGame()
        {
            grs = GameRenderState.Playing;
            CurrentState = GameState.NewGame(10,10,10,4,4,2);
        }
    }

    public class GameSettings
    {
        public List<Difficulty> Difficulties;
        public Dictionary<string, ConsoleColor> Colors;
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
    
    enum RefreshMode
    {
        Rescale = 2,
        Full = 1,
        ChangesOnly = 0,
    }
}
