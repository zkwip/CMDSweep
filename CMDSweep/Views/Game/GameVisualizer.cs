using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Layout.Text;
using CMDSweep.Layout.Popup;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;
using System;

namespace CMDSweep.Views.Game;

partial class GameVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly IRenderer _renderer;
    private readonly GameSettings _settings;

    private StatBarVisualizer _statBarVisualizer;
    private BoardVisualizer _boardVisualizer;

    private IChangeableTypeVisualizer<TextRenderBox> _textPopupVisualizer;
    private IChangeableTypeVisualizer<TextEnterDialog> _enterHighscorePopupVisualizer;
    private IChangeableTypeVisualizer<HighscoreTable> _showHighscorePopupVisualizer;

    private TextRenderBox _winPopupTextBox;
    private TextRenderBox _losePopupTextBox;

    private StyleData _hideStyle;

    public GameVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _settings = settings;

        _hideStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");


        _statBarVisualizer = new StatBarVisualizer(_renderer, settings);
        _boardVisualizer = new BoardVisualizer(_renderer, settings);

        PreparePopups(settings);
    }

    private void PreparePopups(GameSettings settings)
    {
        StyleData popupStyle = settings.GetStyle("popup");
        StyleData textEnterStyle = settings.GetStyle("popup-textbox");

        _textPopupVisualizer = new PopupVisualizer<TextRenderBox>(_renderer, settings, new TextRenderBoxVisualizer(_renderer, settings, popupStyle));

        _winPopupTextBox = new TextRenderBox
        {
            Text = settings.Texts["popup-win-message"],
            Dimensions = new Dimensions(settings.Dimensions["popup-message-width"], settings.Dimensions["popup-message-height"]),
            HorizontalAlign = Layout.HorzontalAlignment.Center,
            VerticalAlign = Layout.VerticalAlignment.Middle
        };

        _losePopupTextBox = new TextRenderBox
        {
            Text = settings.Texts["popup-lose-message"],
            Dimensions = new Dimensions(settings.Dimensions["popup-message-width"], settings.Dimensions["popup-message-height"]),
            HorizontalAlign = Layout.HorzontalAlignment.Center,
            VerticalAlign = Layout.VerticalAlignment.Middle
        };


        _showHighscorePopupVisualizer = new PopupVisualizer<HighscoreTable>(_renderer, settings, new HighscoreTableVisualizer(_renderer, popupStyle, settings.GetStyle("popup-fg-highlight", "popup-bg")));
        _enterHighscorePopupVisualizer = new PopupVisualizer<TextEnterDialog>(_renderer, settings, new TextEnterDialogVisualizer(_renderer, settings, popupStyle, textEnterStyle));
    }

    public void Visualize(GameState state)
    {
        _renderer.ClearScreen(_hideStyle);
        _boardVisualizer.Visualize(state);
        _statBarVisualizer.Visualize(state);
        _renderer.HideCursor(_hideStyle);

        VisualizePopups(state);
    }

    private void VisualizePopups(GameState state)
    {
        switch (state.PlayerState)
        {
            case PlayerState.Win:
                _textPopupVisualizer.Visualize(_winPopupTextBox);
                break;

            case PlayerState.Dead:
                _textPopupVisualizer.Visualize(_losePopupTextBox);
                break;

            case PlayerState.ShowingHighscores:
                _showHighscorePopupVisualizer.Visualize(new HighscoreTable(state.Difficulty, _settings));
                break;

            case PlayerState.EnteringHighscore:
                _enterHighscorePopupVisualizer.Visualize(state.EnteredNameDialog);
                break;
        }
    }

    public void VisualizeChanges(GameState state, GameState previousState)
    {
        _boardVisualizer.VisualizeChanges(state, previousState);
        _statBarVisualizer.Visualize(state);
        _statBarVisualizer.Visualize(state);

        if (state.PlayerState != previousState.PlayerState)
            VisualizePopups(state);
        else
            VisualizePopupChanges(state, previousState);

        Console.Title = $"PlayerState: {state.PlayerState}";
    }

    private void VisualizePopupChanges(GameState state, GameState previousState)
    {
        switch (state.PlayerState)
        {
            case PlayerState.Win:
                _textPopupVisualizer.Visualize(_winPopupTextBox);
                break;

            case PlayerState.Dead:
                _textPopupVisualizer.Visualize(_losePopupTextBox);
                break;

            case PlayerState.ShowingHighscores:
                _showHighscorePopupVisualizer.Visualize(new HighscoreTable(state.Difficulty, _settings));
                break;

            case PlayerState.EnteringHighscore:
                _enterHighscorePopupVisualizer.VisualizeChanges(state.EnteredNameDialog, previousState.EnteredNameDialog);
                break;
        }
    }
}
