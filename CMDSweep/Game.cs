using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace CMDSweep
{
    using Control = KeyValuePair<InputAction, List<ConsoleKey>>;

    public class GameApp
    {
        enum ApplicationState
        {
            Playing,
            Menu,
            Done,
            Highscore,
        }

        Timer refreshTimer;
        Bounds screenBounds;

        private ApplicationState appState;

        internal readonly GameSettings Settings;
        internal readonly IRenderer Renderer;
        internal readonly BoardVisualizer BVis;
        internal readonly MenuVisualizer MVis;

        public GameBoardState CurrentState;
        public Difficulty CurrentDifficulty;

        public GameApp(IRenderer r)
        {
            // Set up
            Settings = LoadSettings();
            CurrentDifficulty = Settings.Difficulties[3];
            Renderer = r;
            screenBounds = r.Bounds;

            MVis = new MenuVisualizer(this);
            BVis = new BoardVisualizer(this);
            

            refreshTimer = new Timer(100);
            refreshTimer.Elapsed += TimerElapsed;
            refreshTimer.AutoReset = true;

            // Start a new game
            //InitialiseGame();
            InitialiseMenu();
            
            Refresh(RefreshMode.Rescale);

            while (Step());
            refreshTimer.Stop();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e) => Refresh(RefreshMode.ChangesOnly);

        private bool Step()
        {
            InputAction ia = ReadAction();
            switch (appState)
            {
                case ApplicationState.Playing: return PlayStep(ia);
                case ApplicationState.Done: return DoneStep(ia);
                case ApplicationState.Highscore: return HighScoreStep(ia);
                case ApplicationState.Menu: return MenuStep(ia);
                default: return false;
            }
        }

        private InputAction ReadAction()
        {
            ConsoleKey key = Console.ReadKey().Key;
            foreach (Control ctrl in Settings.Controls)
                if (ctrl.Value.Contains(key))
                    return ctrl.Key;
            return InputAction.Unknown;
        }

        private bool PlayStep(InputAction ia)
        {
            CurrentState.Face = Face.Normal;
            switch (ia)
            {
                case InputAction.Up:
                    CurrentState = CurrentState.Clone();
                    CurrentState.MoveCursor(Direction.Up);
                    break;
                case InputAction.Down:
                    CurrentState = CurrentState.Clone();
                    CurrentState.MoveCursor(Direction.Down);
                    break;
                case InputAction.Left:
                    CurrentState = CurrentState.Clone();
                    CurrentState.MoveCursor(Direction.Left);
                    break;
                case InputAction.Right:
                    CurrentState = CurrentState.Clone();
                    CurrentState.MoveCursor(Direction.Right);
                    break;
                case InputAction.Dig:
                    CurrentState = CurrentState.Clone();
                    CurrentState.Dig();
                    break;
                case InputAction.Flag:
                    CurrentState = CurrentState.Clone();
                    CurrentState.ToggleFlag();
                    break;
                case InputAction.Quit:
                    return false;
                case InputAction.NewGame:
                    InitialiseGame();
                    break;
            }

            if (CurrentState.PlayerState == PlayerState.Dead || CurrentState.PlayerState == PlayerState.Win)
            {
                refreshTimer.Stop();
                Refresh(RefreshMode.Full);
                appState = ApplicationState.Done;
            }
            else
            {
                if (!refreshTimer.Enabled) refreshTimer.Start();
                Refresh(RefreshMode.ChangesOnly);
            }

            return true;
        }

        private bool DoneStep(InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Quit:
                    return false;
                case InputAction.Dig:
                case InputAction.NewGame:
                    InitialiseGame();
                    Refresh(RefreshMode.Full);
                    break;
            }
            return true;
        }

        private bool HighScoreStep(InputAction ia)
        {
            return false; // TODO
        }

        private bool MenuStep(InputAction ia)
        {
            MVis.Visualize(RefreshMode.ChangesOnly);
            switch (ia)
            {
                case InputAction.Quit:
                    return false;
                case InputAction.Dig:
                case InputAction.NewGame:
                    InitialiseGame();
                    Refresh(RefreshMode.Full);
                    break;
            }
            return true; // TODO
        }

        private GameSettings LoadSettings()
        {
            using (StreamReader r = new StreamReader("settings.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<GameSettings>(json);
            }
        }

        
        private void Refresh(RefreshMode mode)
        {
            if (screenBounds != Renderer.Bounds)
            {
                mode = RefreshMode.Rescale;
                screenBounds = Renderer.Bounds;
            }

            switch (appState)
            {

                case ApplicationState.Playing:
                case ApplicationState.Done:
                    BVis.Visualize(mode);
                    break;
                default:
                    break;
            }

        }

        public void InitialiseGame()
        {
            appState = ApplicationState.Playing;
            CurrentState = GameBoardState.NewGame(CurrentDifficulty);
        }

        public void InitialiseMenu()
        {
            appState = ApplicationState.Menu;
            MVis.Visualize(RefreshMode.Full);
        }
    }

    internal class GameSettings
    {
        public List<Difficulty> Difficulties;
        public Dictionary<string, ConsoleColor> Colors;
        public Dictionary<string, string> Texts;
        public Dictionary<string, int> Dimensions;
        public Dictionary<InputAction, List<ConsoleKey>> Controls;
    }

    public class Difficulty
    {
        public string Name;

        public int Width;
        public int Height;
        public int Mines;
        public int Lives;
        public int Safezone;
        public int DetectionRadius;

        public bool FlagsAllowed;
        public bool QuestionMarkAllowed;
        public bool WrapAround;
        public bool SubtractFlags;
        public bool OnlyShowAtCursor;
        public bool AutomaticDiscovery;
    }

    public enum RefreshMode
    {
        Rescale = 2,
        Full = 1,
        ChangesOnly = 0,
    }

    enum InputAction
    {
        Up,
        Left,
        Down,
        Right,

        Dig,
        Flag,

        Quit,
        NewGame,
        Help,
        Cheat,

        Unknown,

        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Zero,
    }

    public enum Face
    {
        Normal,
        Surprise,
        Win,
        Dead,
    }

}
