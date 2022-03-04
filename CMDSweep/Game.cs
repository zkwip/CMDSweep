using System;
using System.Collections.Generic;
using System.Timers;

namespace CMDSweep;

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

    private readonly Timer refreshTimer;

    private ApplicationState appState;

    // Modules
    internal readonly GameSettings Settings;
    internal SaveData SaveData;
    internal readonly IRenderer Renderer;
    internal readonly BoardVisualizer BVis;
    internal readonly MenuVisualizer MVis;

    // Curent States
    internal MenuList currentMenuList;
    internal GameBoardState CurrentState;

    // Sorta Globals
    internal MenuList MainMenu;
    internal MenuList SettingsMenu;
    internal MenuList AdvancedSettingsMenu;

    public GameApp(IRenderer r)
    {
        // Set up
        Settings = Storage.LoadSettings();
        SaveData = Storage.LoadSaveFile(Settings);
        //CurrentDifficulty = SaveData.CurrentDifficulty;
        Renderer = r;

        MVis = new MenuVisualizer(this);
        BVis = new BoardVisualizer(this);

        BuildMenus();

        refreshTimer = new Timer(100);
        refreshTimer.Elapsed += TimerElapsed;
        refreshTimer.AutoReset = true;

        OpenMenu(MainMenu);

        Renderer.BoundsChanged += Renderer_BoundsChanged;
        while (Step());
        refreshTimer.Stop();

    }

    private void Renderer_BoundsChanged(object? sender, EventArgs e)
    {
        if (e is BoundsChangedEventArgs _) Refresh(RefreshMode.Full);
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
        ConsoleKey key = Console.ReadKey(true).Key;
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

        if (CurrentState.PlayerState == PlayerState.Playing)
        {
            if (!refreshTimer.Enabled) refreshTimer.Start();
            Refresh(RefreshMode.ChangesOnly);
        }
        else if(CurrentState.PlayerState == PlayerState.Dead || CurrentState.PlayerState == PlayerState.Win)
        {
            refreshTimer.Stop();
            if (CurrentState.PlayerState == PlayerState.Win) CurrentState.highscore = CheckHighscore(CurrentState);
            Refresh(RefreshMode.Full);
            appState = ApplicationState.Done;
        }
        else
        {
            Refresh(RefreshMode.ChangesOnly);
        }

        return true;
    }

    private bool CheckHighscore(GameBoardState currentState)
    {
        TimeSpan time = currentState.Time;
        List<HighscoreRecord> scores = SaveData.CurrentDifficulty.Highscores;

        if (scores.Count >= Highscores.highscoreEntries)
        {
            if (time < scores[Highscores.highscoreEntries - 1].Time) scores.RemoveAt(Highscores.highscoreEntries - 1);
        }

        if (scores.Count < Highscores.highscoreEntries)
        {
            scores.Add(new() { 
                Time = time, 
                Name = "Test", 
                Date = DateTime.Now 
            });

            scores.Sort((x, y) => (x.Time - y.Time).Milliseconds);
            Storage.WriteSave(SaveData);

            return true;
        }
        return false;
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

    private void Refresh(RefreshMode mode)
    {
        if (appState != ApplicationState.Playing) refreshTimer.Stop();

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
        CurrentState = GameBoardState.NewGame(SaveData.CurrentDifficulty);
        Storage.WriteSave(SaveData);

        Refresh(RefreshMode.Full);
    }

    public void BuildMenus()
    {
        MainMenu = new("Main Menu", this);

        SettingsMenu = new("Settings", this);
        SettingsMenu.ParentMenu = MainMenu;

        AdvancedSettingsMenu = new("Advanced", this);
        AdvancedSettingsMenu.ParentMenu = SettingsMenu;

        MainMenu.AddButton("New Game", () => InitialiseGame());
        MainMenu.AddButton("High Scores", () => ShowHighscores());
        MainMenu.AddButton("Help", () => ShowHelp());
        MainMenu.AddButton("Settings", () => OpenMenu(SettingsMenu));
        MainMenu.AddButton("Quit Game", () => QuitGame());

        CreateSettingsItem(SettingsMenu, new MenuChoice<Difficulty>("Difficulty", SaveData.Difficulties, x => x.Name), x => x, (d, val) => SaveData.CurrentDifficulty = val);

        CreateSettingsItem(SettingsMenu, new MenuNumberRange("Width", 5, 1000), x => x.Width, (d, val) => d.Width = val);
        CreateSettingsItem(SettingsMenu, new MenuNumberRange("Height", 5, 1000), x => x.Height, (d, val) => d.Height = val);
        CreateSettingsItem(SettingsMenu, new MenuNumberRange("Mines", 1, 10000), x => x.Mines, (d, val) => d.Mines = val);
        CreateSettingsItem(SettingsMenu, new MenuNumberRange("Lives", 1, 100), x => x.Lives, (d, val) => d.Lives = val);

        SettingsMenu.AddButton("Advanced", () => OpenMenu(AdvancedSettingsMenu));

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

    private void CreateSettingsItem<TOption>(MenuList Parent, MenuChoice<TOption> Item, Func<Difficulty, TOption> ReadProperty, Action<Difficulty, TOption> WriteProperty)
    {
        // Changing the difficulty itself works differently
        if (Item is MenuChoice<Difficulty> choice)
            Item.ValueChanged += (i, o) => ChangePreset(choice);
        else
            Item.ValueChanged += (i, o) => ForkCurrentDifficulty(WriteProperty, Item.SelectedOption); 

        // Change the value when the difficulty is changed
        DifficultyChanged += (i, o) => Item.SelectValue(ReadProperty(SaveData.CurrentDifficulty));

        if (SaveData.CurrentDifficulty == null)
            Item.SelectValue(ReadProperty(SaveData.Difficulties[0]));
        else
            Item.SelectValue(ReadProperty(SaveData.CurrentDifficulty));


        Parent.Add(Item);
    }

    private void ForkCurrentDifficulty<TOption>(Action<Difficulty, TOption> Write, TOption Value)
    {
        // Create a new custom difficulty if you are changing from another preset
        if (SaveData.CurrentDifficulty.Name != "Custom")
        {
            SaveData.CurrentDifficulty = SaveData.CurrentDifficulty.Clone("Custom");
            SaveData.Difficulties.RemoveAll(dif => dif.Name == "Custom");
            SaveData.Difficulties.Add(SaveData.CurrentDifficulty);
        }

        Write(SaveData.CurrentDifficulty, Value);
        DifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ChangePreset(MenuChoice<Difficulty> choice) 
    {
        Difficulty d = choice.SelectedOption;
        if (d == null) return;
        if (d.Name == "Custom") return;

        SaveData.CurrentDifficulty = d;
        DifficultyChanged?.Invoke(this, EventArgs.Empty);
    }
    private void ShowHelp()
    {
        throw new NotImplementedException();
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

public enum RefreshMode
{
    None = 0,
    ChangesOnly = 1,
    Scroll = 2,
    Full = 3,
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

