using CMDSweep.Data;
using CMDSweep.IO;
using CMDSweep.Rendering;
using CMDSweep.Views;
using CMDSweep.Views.Menus;
using CMDSweep.Views.Menus.MenuItems;
using System;

namespace CMDSweep;
class MenuController : IViewController
{
    private MenuList _currentMenuList;
    private MenuVisualizer _visualizer;

    public MineApp App { get; }

    public MenuList MainMenu;
    public MenuList SettingsMenu;
    public MenuList AdvancedSettingsMenu;


    public MenuController(MineApp app)
    {
        App = app;
        _visualizer = new MenuVisualizer(Settings, app.Renderer);
        BuildMenus();
    }

    public GameSettings Settings => App.Settings;
    public SaveData SaveData => App.SaveData;

    public void Step()
    {
        InputAction ia = App.ReadAction();
        if (ia == InputAction.NewGame)
        {
            App.GameController.NewGame();
            return;
        }
        _currentMenuList.HandleInput(ia);
    }

    internal void BuildMenus()
    {
        MainMenu = new("Main Menu", this);

        SettingsMenu = new("Settings", this);
        SettingsMenu.ParentMenu = MainMenu;

        AdvancedSettingsMenu = new("Advanced", this);
        AdvancedSettingsMenu.ParentMenu = SettingsMenu;

        MainMenu.AddButton("New Game", () => App.GameController.NewGame());
        MainMenu.AddButton("High Scores", () => App.HighscoreController.ShowHighscores());
        MainMenu.AddButton("Help", () => App.ShowHelp());
        MainMenu.AddButton("Settings", () => OpenMenu(SettingsMenu));
        MainMenu.AddButton("Quit Game", () => App.QuitGame());

        CreateSettingsItem(SettingsMenu, new OptionMenuItem<Difficulty>("Difficulty", SaveData.Difficulties, x => x.Name, Settings), x => x, (d, val) => SaveData.CurrentDifficulty = val);

        CreateSettingsItem(SettingsMenu, new NumberRangeMenuItem("Width", 5, 1000, Settings), x => x.Width, (d, val) => d.Width = val);
        CreateSettingsItem(SettingsMenu, new NumberRangeMenuItem("Height", 5, 1000, Settings), x => x.Height, (d, val) => d.Height = val);
        CreateSettingsItem(SettingsMenu, new NumberRangeMenuItem("Mines", 1, 10000, Settings), x => x.Mines, (d, val) => d.Mines = val);
        CreateSettingsItem(SettingsMenu, new NumberRangeMenuItem("Lives", 1, 100, Settings), x => x.Lives, (d, val) => d.Lives = val);

        SettingsMenu.AddButton("Advanced", () => OpenMenu(AdvancedSettingsMenu));

        CreateSettingsItem(AdvancedSettingsMenu, new NumberRangeMenuItem("Safe Zone", 1, 100, Settings), x => x.Safezone, (d, val) => d.Safezone = val);
        CreateSettingsItem(AdvancedSettingsMenu, new NumberRangeMenuItem("Counting Radius", 1, 100, Settings), x => x.DetectionRadius, (d, val) => d.DetectionRadius = val);

        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Counting Wraps Around", Settings), x => x.WrapAround, (d, val) => d.WrapAround = val);
        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Flags Allowed", Settings), x => x.FlagsAllowed, (d, val) => d.FlagsAllowed = val);
        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Question Marks Allowed", Settings), x => x.QuestionMarkAllowed, (d, val) => d.QuestionMarkAllowed = val);
        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Automatic Discovery", Settings), x => x.AutomaticDiscovery, (d, val) => d.AutomaticDiscovery = val);
        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Subtract Flags From Count", Settings), x => x.SubtractFlags, (d, val) => d.SubtractFlags = val) ;
        CreateSettingsItem(AdvancedSettingsMenu, new BoolOptionMenuItem("Only Show Numbers At Cursor", Settings), x => x.OnlyShowAtCursor, (d, val) => d.OnlyShowAtCursor = val);
    }

    private event EventHandler DifficultyChanged;

    private void CreateSettingsItem<TOption>(MenuList Parent, OptionMenuItem<TOption> Item, Func<Difficulty, TOption> ReadProperty, Action<Difficulty, TOption> WriteProperty)
    {
        // Changing the difficulty itself works differently
        if (Item is OptionMenuItem<Difficulty> choice)
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
        if (App.SaveData.CurrentDifficulty.Name != "Custom")
        {
            SaveData.CurrentDifficulty = SaveData.CurrentDifficulty.Clone("Custom");
            SaveData.Difficulties.RemoveAll(dif => dif.Name == "Custom");
            SaveData.Difficulties.Add(SaveData.CurrentDifficulty);
        }

        Write(SaveData.CurrentDifficulty, Value);
        DifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ChangePreset(OptionMenuItem<Difficulty> choice)
    {
        Difficulty d = choice.SelectedOption;
        if (d == null) return;
        if (d.Name == "Custom") return;

        SaveData.CurrentDifficulty = d;
        DifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OpenMain() => OpenMenu(MainMenu);
    public void OpenMenu(MenuList? menu)
    {
        if (menu == null) return;
        _currentMenuList = menu;
        App.ChangeMode(ApplicationState.Menu);
    }

    public void ResizeView() => _visualizer.Resize();

    public void Refresh(RefreshMode mode) => _visualizer.Visualize(_currentMenuList);
}
