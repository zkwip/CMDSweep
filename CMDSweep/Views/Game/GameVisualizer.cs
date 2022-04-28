using CMDSweep.Data;
using CMDSweep.Layout.Text;
using CMDSweep.Layout.Popup;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;

namespace CMDSweep.Views.Game;

partial class GameVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly IRenderer _renderer;
    private readonly GameSettings _settings;

    private StatBarVisualizer _statBarVisualizer;
    private BoardVisualizer _boardVisualizer;

    private IChangeableTypeVisualizer<TextRenderBox> _textPopupVisualizer;
    private IChangeableTypeVisualizer<string> _enterHighscorePopupVisualizer;
    private IChangeableTypeVisualizer<HighscoreTable> _showHighscorePopupVisualizer;

    private TextRenderBox _winPopupTextBox;
    private TextRenderBox _losePopupTextBox;

    private StyleData _hideStyle;

    public GameVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _settings = settings;

        _hideStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        StyleData popupStyle = settings.GetStyle("popup");

        _statBarVisualizer = new StatBarVisualizer(_renderer, settings);
        _boardVisualizer = new BoardVisualizer(_renderer, settings);

        PreparePopups(popupStyle, settings);
    }

    private void PreparePopups(StyleData popupStyle, GameSettings settings)
    {
        _textPopupVisualizer = new PopupVisualizer<TextRenderBox>(_renderer, settings, new TextRenderBoxVisualizer(_renderer, settings, popupStyle));
        _winPopupTextBox = new TextRenderBox();
        _winPopupTextBox.Text = "Congratulations, You won!\n\nYou can play again by pressing any key.";
        _losePopupTextBox = new TextRenderBox();
        _losePopupTextBox.Text = "You died!\n\nYou can play again by pressing any key.";

        _showHighscorePopupVisualizer = new PopupVisualizer<HighscoreTable>(_renderer, settings, new HighscoreTableVisualizer(_renderer, popupStyle, settings.GetStyle("popup-fg-highlight", "popup-bg")));
    }

    public void Visualize(GameState state)
    {
        _renderer.ClearScreen(_hideStyle);
        _boardVisualizer.Visualize(state);
        _statBarVisualizer.Visualize(state);
        _renderer.HideCursor(_hideStyle);

        RenderPopups(state);
    }

    private void RenderPopups(GameState state)
    {
        switch (state.ProgressState.PlayerState)
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
                //_enterHighscorePopupVisualizer.Visualize(state.PlayerName);
                break;
        }
    }

    public void VisualizeChanges(GameState state, GameState previousState)
    {
        _boardVisualizer.VisualizeChanges(state, previousState);
        _statBarVisualizer.Visualize(state);
        _statBarVisualizer.Visualize(state);
    }
}
