﻿using System;
using System.Collections.Generic;
using System.Timers;

namespace CMDSweep;

using Control = KeyValuePair<InputAction, List<ConsoleKey>>;

public class GameApp
{

    // Modules
    internal readonly GameSettings Settings;
    internal SaveData SaveData;
    internal readonly IRenderer Renderer;

    // Curent States
    internal ApplicationState appState;

    // Sorta Globals

    internal BoardController BControl;
    internal HighscoreController HControl;
    internal MenuController MControl;

    internal Controller CurrentController;

    public GameApp(IRenderer r)
    {
        // Set up
        Settings = Storage.LoadSettings();
        SaveData = Storage.LoadSaveFile(Settings);
        Renderer = r;

        BControl = new BoardController(this);
        HControl = new HighscoreController(this);
        MControl = new MenuController(this);
        Renderer.BoundsChanged += Renderer_BoundsChanged;

        MControl.OpenMenu(MControl.MainMenu);
        CurrentController = MControl;

        while (Step());
    }

    private void Renderer_BoundsChanged(object? sender, EventArgs _) => Refresh(RefreshMode.Full);
    private void TimerElapsed(object? sender, ElapsedEventArgs e) => Refresh(RefreshMode.ChangesOnly);
    private bool Step()
    {
        if (appState == ApplicationState.Quit) return false;
        switch (appState)
        {
            case ApplicationState.Playing: return BControl.Step();
            case ApplicationState.Done: return BControl.DoneStep();
            case ApplicationState.Highscore: return HControl.Step();
            case ApplicationState.Menu: return MControl.Step();
            default: return false;
        }
    }

    internal InputAction ReadAction()
    {
        ConsoleKey key = Console.ReadKey(true).Key;
        foreach (Control ctrl in Settings.Controls)
            if (ctrl.Value.Contains(key))
                return ctrl.Key;
        return InputAction.Unknown;
    }
    internal void Refresh(RefreshMode mode)
    {
        switch (appState)
        {
            case ApplicationState.Playing:
            case ApplicationState.Done:
                BControl.Visualize(mode);
                break;
            case ApplicationState.Menu:
                MControl.Visualize(mode);
                break;
            case ApplicationState.Highscore:
                HControl.Visualize(mode);
                break;
            default:
                break;
        }
    }
    internal void ShowHelp() => appState = ApplicationState.Help;
    internal void QuitGame() => appState = ApplicationState.Quit;

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

