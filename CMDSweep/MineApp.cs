using CMDSweep.Data;
using CMDSweep.IO;
using CMDSweep.Rendering;
using CMDSweep.Views;
using CMDSweep.Views.Game;
using CMDSweep.Views.Help;
using CMDSweep.Views.Highscore;
using System;
using System.Collections.Generic;

namespace CMDSweep;

class MineApp
{
    // Modules
    internal readonly GameSettings Settings;
    internal SaveData SaveData;
    internal readonly IRenderer Renderer;

    internal GameController GameController;
    internal HighscoreController HighscoreController;
    internal HelpController HelpController;
    internal MenuController MenuController;

    // Curent States
    internal ApplicationState AppState;

    internal IViewController CurrentController => AppState switch
    {
        ApplicationState.Playing => GameController,
        ApplicationState.Done => GameController,
        ApplicationState.Highscore => HighscoreController,
        ApplicationState.Menu => MenuController,
        ApplicationState.Help => HelpController,
        _ => MenuController
    };

    static void Main(string[] _)
    {
        IRenderer cmdr = new WinCMDRenderer();
        MineApp app = new(cmdr);
        while (app.Step());
    }

    public MineApp(IRenderer r)
    {
        // Set up
        Settings = Storage.LoadSettings();
        SaveData = Storage.LoadSaveFile(Settings);
        Renderer = r;

        GameController = new(this);
        HighscoreController = new(this);
        HelpController = new(this);
        MenuController = new(this);

        Renderer.BoundsChanged += Renderer_BoundsChanged;

        MenuController.OpenMenu(MenuController.MainMenu);
    }

    private void Renderer_BoundsChanged(object? sender, EventArgs _) => CurrentController!.ResizeView();

    private bool Step()
    {
        CurrentController.Step();
        return Refresh(RefreshMode.ChangesOnly);
    }

    internal bool Refresh(RefreshMode mode)
    {
        if (AppState == ApplicationState.Quit) 
            return false;

        if (CurrentController == null) 
            return false;

        CurrentController.Refresh(mode);
        return true;
    }

    internal void ShowMainMenu() => MenuController.OpenMain();
    
    internal void ShowHelp() => ChangeMode(ApplicationState.Help);
    
    internal void QuitGame() => ChangeMode(ApplicationState.Quit);
    
    internal void ContinueGame() => ChangeMode(ApplicationState.Playing);

    internal void ChangeMode(ApplicationState state)
    {
        AppState = state;
        Refresh(RefreshMode.Full);
    }
}
