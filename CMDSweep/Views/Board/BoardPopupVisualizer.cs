using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Layout.Popup;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;

namespace CMDSweep.Views.Board;

internal class BoardPopupVisualizer : ITypeVisualizer<BoardState>
{
    private PopupVisualizer _popupVisualizer;
    private GameSettings _settings;
    private IRenderer _renderer;
    private StyleData _styleData;
    private Difficulty _difficulty;

    public IPopup HighscoreTablePopup;
    public IPopup EnterHighscorePopup;
    public IPopup YouWinPopup;
    public IPopup YouLosePopup;

    public BoardPopupVisualizer(IRenderer renderer, GameSettings settings)
    {
        _popupVisualizer = new PopupVisualizer(renderer, settings);
        _settings = settings;
        _renderer = renderer;
        _styleData = settings.GetStyle("popup");

        PreparePopups();
    }

    private void PreparePopups()
    {
        HighscoreTablePopup = PrepareHighscoreTablePopup();
        EnterHighscorePopup = PrepareEnterHighscorePopup();
        YouWinPopup = PrepareTextPopup("Congratulations, You won!\n\nYou can play again by pressing any key.");
        YouLosePopup = PrepareTextPopup("You died!\n\nYou can play again by pressing any key.");
    }

    public void Visualize(BoardState state)
    {
        _difficulty = state.Difficulty;
        IPopup? popup = (state.RoundState.PlayerState) switch
        {
            PlayerState.Win => _popupVisualizer.Visualize(YouWinPopup),
            PlayerState.Dead => _popupVisualizer.Visualize(YouLosePopup),
            PlayerState.ShowingHighscores => _popupVisualizer.Visualize(HighscoreTablePopup),
            PlayerState.EnteringHighscore => _popupVisualizer.Visualize(EnterHighscorePopup),
        };
        _popupVisualizer.Visualize(popup);
    }

    private IPopup PrepareTextPopup(string text) => new TextPopup(_settings, text, default);

    private IPopup PrepareEnterHighscorePopup()
    {

        TableGrid hsTableGrid = new TableGrid();

        hsTableGrid.AddColumn(_settings.Dimensions["popup-enter-hs-width"], 0);
        hsTableGrid.AddRow(2, 0);
        hsTableGrid.AddRow(1, 0);
        hsTableGrid.FitAround(0);

        TextEnterField textEnterField = new(hsTableGrid.GetCell(0,0), _styleData);
        TextRenderBox textRenderBox = new TextRenderBox(_settings.Texts["popup-enter-hs-message"], hsTableGrid.GetCell(0, 0), _styleData);

        return new TableGridPopup(hsTableGrid, _styleData, (tableGrid, renderer) => {
            textEnterField.Bounds = tableGrid.GetCell(0, 1);
            textRenderBox.Bounds = tableGrid.GetCell(0, 0);

            textRenderBox.Render(renderer, false);
        });
    }

    private IPopup PrepareHighscoreTablePopup()
    {
        TableGrid highscoreTableGrid = HighscoreTable.GetHSTableGrid(_settings);

        return new TableGridPopup(highscoreTableGrid, _styleData, (tableGrid, renderer) => {
            HighscoreTable.RenderHSTable(renderer, _settings, tableGrid, _difficulty, _styleData);
        });
    }
}
