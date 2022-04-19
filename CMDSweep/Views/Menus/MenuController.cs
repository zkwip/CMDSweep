using CMDSweep.IO;
using CMDSweep.Views.Menus;
using System;

namespace CMDSweep;
class MenuController : Controller
{
    internal MenuList currentMenuList;

    internal MenuList MainMenu;
    internal MenuList SettingsMenu;
    internal MenuList AdvancedSettingsMenu;
    public MenuController(GameApp app) : base(app)
    {
        Visualizer = new MenuVisualizer(this);
        BuildMenus();
    }

    internal override bool Step()
    {
        InputAction ia = App.ReadAction();
        if (ia == InputAction.NewGame)
        {
            App.BControl.NewGame();
            return true;
        }
        bool res = currentMenuList.HandleInput(ia);
        App.Refresh(RefreshMode.ChangesOnly);
        return res;
    }

    internal void BuildMenus()
    {
        MainMenu = new("Main Menu", this);

        SettingsMenu = new("Settings", this);
        SettingsMenu.ParentMenu = MainMenu;

        AdvancedSettingsMenu = new("Advanced", this);
        AdvancedSettingsMenu.ParentMenu = SettingsMenu;

        MainMenu.AddButton("New Game", () => App.BControl.NewGame());
        MainMenu.AddButton("High Scores", () => App.HSControl.ShowHighscores());
        MainMenu.AddButton("Help", () => App.ShowHelp());
        MainMenu.AddButton("Settings", () => OpenMenu(SettingsMenu));
        MainMenu.AddButton("Quit Game", () => App.QuitGame());

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
        if (App.SaveData.CurrentDifficulty.Name != "Custom")
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

    public void OpenMain() => OpenMenu(MainMenu);
    public void OpenMenu(MenuList? menu)
    {
        if (menu == null) return;
        currentMenuList = menu;
        App.ChangeMode(ApplicationState.Menu);
    }
}
