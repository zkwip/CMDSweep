using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

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
            Quit,
        }

        Timer refreshTimer;
        Bounds screenBounds;

        private ApplicationState appState;

        internal readonly GameSettings Settings;
        internal readonly IRenderer Renderer;
        internal readonly BoardVisualizer BVis;
        internal readonly MenuVisualizer MVis;

        public MenuList currentMenuList;
        public GameBoardState CurrentState;
        public Difficulty CurrentDifficulty;

        public MenuList MainMenu;
        public MenuList SettingsMenu;

        public GameApp(IRenderer r)
        {
            // Set up
            Settings = LoadSettings();
            CurrentDifficulty = Settings.Difficulties[3];
            Renderer = r;
            screenBounds = r.Bounds;

            MVis = new MenuVisualizer(this);
            BVis = new BoardVisualizer(this);

            BuildMenus();

            refreshTimer = new Timer(100);
            refreshTimer.Elapsed += TimerElapsed;
            refreshTimer.AutoReset = true;

            // Testing
            OpenMenu(MainMenu);

            while (Step()) ;
            refreshTimer.Stop();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e) => Refresh(RefreshMode.ChangesOnly);

        private bool Step()
        {
            if (appState == ApplicationState.Quit) return false;
            InputAction ia = ReadAction();
            switch (appState)
            {
                case ApplicationState.Playing: return PlayStep(ia);
                case ApplicationState.Done: return DoneStep(ia);
                case ApplicationState.Highscore: return HighScoreStep(ia);
                case ApplicationState.Menu: return MenuStep(ia);

                case ApplicationState.Quit:
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
                    OpenMenu(MainMenu);
                    break;
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
                    OpenMenu(MainMenu);
                    break;
                case InputAction.Dig:
                case InputAction.NewGame:
                    InitialiseGame();
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
            bool res = currentMenuList.HandleInput(ia); // TODO
            Refresh(RefreshMode.ChangesOnly);
            return res;
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
            if (appState != ApplicationState.Playing) refreshTimer.Stop();

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
                case ApplicationState.Menu:
                    MVis.Visualize(mode);
                    break;
                default:
                    break;
            }
        }

        public void InitialiseGame()
        {
            refreshTimer.Stop();
            appState = ApplicationState.Playing;
            CurrentState = GameBoardState.NewGame(CurrentDifficulty);
            Refresh(RefreshMode.Full);
        }

        public void BuildMenus()
        {
            MainMenu = new MenuList("Main Menu", this);

            SettingsMenu = new MenuList("Settings", this);
            SettingsMenu.ParentMenu = MainMenu;

            MenuItem StartButton = new MenuButton("Start New Game");
            StartButton.ValueChanged += (i, o) => InitialiseGame();
            MainMenu.Add(StartButton);

            MenuItem HighButton = new MenuButton("High scores");
            HighButton.ValueChanged += (i, o) => ShowHighscores();
            MainMenu.Add(HighButton);

            MenuItem SettingsButton = new MenuButton("Settings");
            SettingsButton.ValueChanged += (i, o) => OpenMenu(SettingsMenu);
            MainMenu.Add(SettingsButton);

            MenuItem QuitButton = new MenuButton("Quit");
            QuitButton.ValueChanged += (i, o) => QuitGame();
            MainMenu.Add(QuitButton);


            SettingsMenu.Add(new MenuChoice<Difficulty>("Difficulty", Settings.Difficulties, x => x.Name));
            SettingsMenu.Add(new MenuNumberRange("Width", 5, 1000));
            SettingsMenu.Add(new MenuNumberRange("Height", 5, 1000));
            SettingsMenu.Add(new MenuNumberRange("Mines", 1, 10000));
            SettingsMenu.Add(new MenuNumberRange("Lives", 1, 100));

            SettingsMenu.Add(new MenuText("Advanced"));
            SettingsMenu.Add(new MenuNumberRange("Safe Zone", 1, 100));
            SettingsMenu.Add(new MenuNumberRange("Counting Radius", 1, 100));
            SettingsMenu.Add(new MenuBoolOption("Counting Wraps Around"));
            SettingsMenu.Add(new MenuBoolOption("Flags Allowed"));
            SettingsMenu.Add(new MenuBoolOption("Question Marks Allowed"));
            SettingsMenu.Add(new MenuBoolOption("Automatic Discovery"));
            SettingsMenu.Add(new MenuBoolOption("Subtract Flags From Count"));
            SettingsMenu.Add(new MenuBoolOption("Only Show Numbers At Cursor"));
        }

        private void ShowHighscores()
        {
            throw new NotImplementedException();
        }

        private void QuitGame()
        {
            appState = ApplicationState.Quit;
        }

        public void OpenMenu(MenuList menu)
        {
            refreshTimer.Stop();
            currentMenuList = menu;
            appState = ApplicationState.Menu;
            Refresh(RefreshMode.Full);
        }

        public void ContinueGame()
        {
            appState = ApplicationState.Playing;
            Refresh(RefreshMode.Full);
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
        Clear,
    }

    public enum Face
    {
        Normal,
        Surprise,
        Win,
        Dead,
    }

}
