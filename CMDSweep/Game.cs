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
        public MenuList AdvancedSettingsMenu;

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
        private void TimerElapsed(object? sender, ElapsedEventArgs e) => Refresh(RefreshMode.ChangesOnly);

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

                GameSettings? gs = JsonConvert.DeserializeObject<GameSettings>(json);
                if (gs == null) throw new Exception("Failed to load settings");
                return gs;
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

            Refresh(RefreshMode.Rescale);
            Renderer.SetTitle(appState.ToString());
        }

        public void BuildMenus()
        {
            MainMenu = new MenuList("Main Menu", this);

            SettingsMenu = new MenuList("Settings", this);
            SettingsMenu.ParentMenu = MainMenu;

            AdvancedSettingsMenu = new MenuList("Advanced", this);
            AdvancedSettingsMenu.ParentMenu = SettingsMenu;

            MenuItem StartButton = new MenuButton("Start New Game");
            StartButton.ValueChanged += (i, o) => InitialiseGame();
            MainMenu.Add(StartButton);

            MenuItem HighButton = new MenuButton("High scores");
            HighButton.ValueChanged += (i, o) => ShowHighscores();
            MainMenu.Add(HighButton);

            CreateMenuButton(MainMenu, "Settings", SettingsMenu);

            MenuItem QuitButton = new MenuButton("Quit");
            QuitButton.ValueChanged += (i, o) => QuitGame();
            MainMenu.Add(QuitButton);

            /*
            MenuChoice<Difficulty> DifficultyOption = new MenuChoice<Difficulty>("Difficulty", Settings.Difficulties, x => x.Name);
            DifficultyOption.ValueChanged += (i, o) => ChangePreset(DifficultyOption);
            SettingsMenu.Add(DifficultyOption);
            */

            CreateSettingsItem(SettingsMenu, new MenuChoice<Difficulty>("Difficulty", Settings.Difficulties, x => x.Name), x => x, (d, val) => this.CurrentDifficulty = val);

            CreateSettingsItem(SettingsMenu, new MenuNumberRange("Width", 5, 1000), x => x.Width, (d, val) => d.Width = val);
            CreateSettingsItem(SettingsMenu, new MenuNumberRange("Height", 5, 1000), x => x.Height, (d, val) => d.Height = val);
            CreateSettingsItem(SettingsMenu, new MenuNumberRange("Mines", 1, 10000), x => x.Mines, (d, val) => d.Mines = val);
            CreateSettingsItem(SettingsMenu, new MenuNumberRange("Lives", 1, 100), x => x.Lives, (d, val) => d.Lives = val);

            CreateMenuButton(SettingsMenu, "Advanced", AdvancedSettingsMenu);

            CreateSettingsItem(AdvancedSettingsMenu, new MenuNumberRange("Safe Zone", 1, 100), x => x.Safezone, (d, val) => d.Safezone = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuNumberRange("Counting Radius", 1, 100), x => x.DetectionRadius, (d, val) => d.DetectionRadius = val);

            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Counting Wraps Around"), x => x.WrapAround, (d, val) => d.WrapAround = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Flags Allowed"), x => x.FlagsAllowed, (d, val) => d.FlagsAllowed = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Question Marks Allowed"), x => x.QuestionMarkAllowed, (d, val) => d.QuestionMarkAllowed = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Automatic Discovery"), x => x.AutomaticDiscovery, (d, val) => d.AutomaticDiscovery = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Subtract Flags From Count"), x => x.SubtractFlags, (d, val) => d.SubtractFlags = val);
            CreateSettingsItem(AdvancedSettingsMenu, new MenuBoolOption("Only Show Numbers At Cursor"), x => x.OnlyShowAtCursor, (d, val) => d.OnlyShowAtCursor = val);
        }

        private event EventHandler DifficultyChanged;

        private void CreateMenuButton(MenuList Parent, string title, MenuList Linked)
        {
            MenuItem res = new MenuButton(title);
            res.ValueChanged += (i, o) => OpenMenu(Linked);
            Parent.Add(res);
        }

        private void CreateSettingsItem<TOption>(MenuList Parent, MenuChoice<TOption> Item, Func<Difficulty, TOption> Read, Action<Difficulty, TOption> Write)
        {
            // Changing the difficulty itself works differently
            if (Item is MenuChoice<Difficulty> choice)
                Item.ValueChanged += (i, o) => ChangePreset(choice);
            else
                Item.ValueChanged += (i, o) => { ForkCurrentDifficulty(Write, Item.SelectedOption); };

            // Change the value when the difficulty is changed
            DifficultyChanged += (i, o) => SelectValue(Item, Read(CurrentDifficulty));
            SelectValue(Item, Read(CurrentDifficulty));


            Parent.Add(Item);
        }

        private void SelectValue<TOption>(MenuChoice<TOption> Item, TOption value)
        {
            if (!Item.Select(value, true)) throw new Exception("Selected item not in the list");
        }

        private void ForkCurrentDifficulty<TOption>(Action<Difficulty, TOption> Write, TOption Value)
        {
            // Create a new custom difficulty if you are changing from another preset
            if (CurrentDifficulty.Name != "Custom")
            {
                CurrentDifficulty = CurrentDifficulty.Clone("Custom");
                Settings.Difficulties.RemoveAll(dif => dif.Name == "Custom");
                Settings.Difficulties.Add(CurrentDifficulty);
            }

            Write(CurrentDifficulty, Value);
            DifficultyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChangePreset(MenuChoice<Difficulty> choice) 
        {
            Difficulty d = choice.SelectedOption;
            if (d == null) return;
            if (d.Name == "Custom") return;

            CurrentDifficulty = d;
            DifficultyChanged?.Invoke(this, EventArgs.Empty);
            Renderer.SetTitle("Invoked");
        }
        
        private void ShowHighscores()
        {
            throw new NotImplementedException();
        }

        private void QuitGame()
        {
            appState = ApplicationState.Quit;
        }

        public void OpenMenu(MenuList? menu)
        {
            if (menu == null) return;

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

    #pragma warning disable CS0649 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class GameSettings
    {
        public List<Difficulty> Difficulties;
        public Dictionary<string, ConsoleColor> Colors;
        public Dictionary<string, string> Texts;
        public Dictionary<string, int> Dimensions;
        public Dictionary<InputAction, List<ConsoleKey>> Controls;
    }

    #pragma warning restore CS0649 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        public override bool Equals(object? obj)
        {
            return obj is Difficulty difficulty &&
                   Name == difficulty.Name &&
                   Width == difficulty.Width &&
                   Height == difficulty.Height &&
                   Mines == difficulty.Mines &&
                   Lives == difficulty.Lives &&
                   Safezone == difficulty.Safezone &&
                   DetectionRadius == difficulty.DetectionRadius &&
                   FlagsAllowed == difficulty.FlagsAllowed &&
                   QuestionMarkAllowed == difficulty.QuestionMarkAllowed &&
                   WrapAround == difficulty.WrapAround &&
                   SubtractFlags == difficulty.SubtractFlags &&
                   OnlyShowAtCursor == difficulty.OnlyShowAtCursor &&
                   AutomaticDiscovery == difficulty.AutomaticDiscovery;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Name);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Mines);
            hash.Add(Lives);
            hash.Add(Safezone);
            hash.Add(DetectionRadius);
            hash.Add(FlagsAllowed);
            hash.Add(QuestionMarkAllowed);
            hash.Add(WrapAround);
            hash.Add(SubtractFlags);
            hash.Add(OnlyShowAtCursor);
            hash.Add(AutomaticDiscovery);
            return hash.ToHashCode();
        }

        internal Difficulty Clone() => Clone(this.Name);
        internal Difficulty Clone(string name)
        {
            return new Difficulty()
            {
                Name = name,
                Width = this.Width,
                Height = this.Height,
                Mines = this.Mines,
                Lives = this.Lives,
                Safezone = this.Safezone,
                DetectionRadius = this.DetectionRadius,
                FlagsAllowed = this.FlagsAllowed,
                QuestionMarkAllowed = this.QuestionMarkAllowed,
                WrapAround = this.WrapAround,
                SubtractFlags = this.SubtractFlags,
                OnlyShowAtCursor = this.OnlyShowAtCursor,
                AutomaticDiscovery = this.AutomaticDiscovery
            };
        }
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
