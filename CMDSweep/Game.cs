﻿using System;
using System.Collections.Generic;

namespace CMDSweep;

using Control = KeyValuePair<InputAction, List<ConsoleKey>>;
public class GameApp
{
    // Modules
    internal readonly GameSettings Settings;
    internal SaveData SaveData;
    internal readonly IRenderer Renderer;

    internal BoardController BControl;
    internal HighscoreController HSControl;
    internal HelpController HLControl;
    internal MenuController MControl;

    // Curent States
    internal ApplicationState AppState;
    internal Controller? CurrentController => AppState switch
    {
        ApplicationState.Playing => BControl,
        ApplicationState.Done => BControl,
        ApplicationState.Highscore => HSControl,
        ApplicationState.Menu => MControl,
        ApplicationState.Help => HLControl,
        _ => null,
    };

    public GameApp(IRenderer r)
    {
        // Set up
        Settings = Storage.LoadSettings();
        SaveData = Storage.LoadSaveFile(Settings);
        Renderer = r;

        BControl = new(this);
        HSControl = new(this);
        HLControl = new(this);
        MControl = new(this);
        Renderer.BoundsChanged += Renderer_BoundsChanged;

        MControl.OpenMenu(MControl.MainMenu);

        while (Step());
    }

    private void Renderer_BoundsChanged(object? sender, EventArgs _) => Refresh(RefreshMode.Full);

    private bool Step()
    {
        if (AppState == ApplicationState.Quit) return false;
        if (CurrentController == null) return false;
        return CurrentController.Step();
    }
    internal void Refresh(RefreshMode mode)
    {
        if (AppState == ApplicationState.Quit) return;
        if (CurrentController == null) return;
        CurrentController.Visualize(mode);
    }

    internal InputAction ReadAction()
    {
        ConsoleKey key = Console.ReadKey(true).Key;
        foreach (Control ctrl in Settings.Controls)
            if (ctrl.Value.Contains(key))
                return ctrl.Key;
        return InputAction.Unknown;
    }

    internal void ShowMainMenu() => MControl.OpenMain();
    internal void ShowHelp() => ChangeMode(ApplicationState.Help);
    internal void QuitGame() => ChangeMode(ApplicationState.Quit);
    internal void ContinueGame() => ChangeMode(ApplicationState.Playing);

    internal void ChangeMode(ApplicationState state)
    {
        AppState = state;
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
enum ApplicationState
{
    Playing,
    Menu,
    Done,
    Highscore,
    Quit,
    Help,
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

