using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;

namespace CMDSweep.Views.Board;

internal class StatboardVisualizer : ITypeVisualizer<BoardState>
{
    private readonly IRenderer _renderer;
    private readonly GameSettings _settings;
    private readonly StyleData _hideStyle;
    private TableGrid _tableGrid;
    public StatboardVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _settings = settings;
        _hideStyle = settings.GetStyle("cell-bg-out-of-bounds", "cell-bg-out-of-bounds");
        _tableGrid = new();
    }

    public void Visualize(BoardState state)
    {
        GenerateTableGrid();

        _renderer.ClearScreen(_hideStyle, _tableGrid.Bounds);
        RenderClock(state);
        RenderFace(state);
        RenderLifeCounter(state);
        RenderMineCounter(state);
    }

    private void GenerateTableGrid()
    {
        _tableGrid.Bounds = new(_renderer.Bounds.HorizontalRange, LinearRange.Zero);

        int horpad = _settings.Dimensions["stat-padding-x"];
        int verpad = _settings.Dimensions["stat-padding-y"];
        int vmidpad = _settings.Dimensions["stat-padding-x-in-between"];

        // Rows
        _tableGrid.AddRow(verpad, 0);
        _tableGrid.AddRow(1, 0, "bar");
        _tableGrid.AddRow(verpad, 0);

        // Columns
        _tableGrid.AddColumn(horpad, 0);
        _tableGrid.AddColumn(6, 0, "clock");
        _tableGrid.AddColumn(vmidpad + 5, 0);
        _tableGrid.AddColumn(horpad, 1);

        _tableGrid.AddColumn(4, 0, "face");

        _tableGrid.AddColumn(horpad, 1);
        _tableGrid.AddColumn(5, 0, "lives");
        _tableGrid.AddColumn(vmidpad, 0);
        _tableGrid.AddColumn(6, 0, "mines");
        _tableGrid.AddColumn(horpad, 0);
    }

    private void RenderClock(BoardState state)
    {
        Point clockPosition = _tableGrid.GetPoint("clock", "bar");
        StyleData clockStyle = _settings.GetStyle("stat-mines");
        _renderer.PrintAtTile(clockPosition, clockStyle, state.Timing.Time.ToString(@"\ h\:mm\:ss\ "));
    }

    private void RenderFace(BoardState state)
    {
        Point facePosition = _tableGrid.GetPoint("face", "bar");
        string face = state.RoundState.Face switch
        {
            Face.Surprise => _settings.Texts["face-surprise"],
            Face.Win => _settings.Texts["face-win"],
            Face.Dead => _settings.Texts["face-dead"],
            _ => _settings.Texts["face-normal"],
        };

        StyleData faceStyle = _settings.GetStyle("face");
        _renderer.PrintAtTile(facePosition, faceStyle, face);
    }

    private void RenderMineCounter(BoardState state)
    {
        Point minePosition = _tableGrid.GetPoint("mine", "bar");
        StyleData minesLeftStyle = _settings.GetStyle("stat-mines");
        _renderer.PrintAtTile(minePosition, minesLeftStyle, string.Format(" {0:D3} ", state.MinesLeft));
    }

    private void RenderLifeCounter(BoardState state)
    {
        Point minePosition = _tableGrid.GetPoint("life", "bar");
        string life = _settings.Texts["stat-life"];

        StyleData livesLeftStyle = _settings.GetStyle("stat-mines");
        StyleData livesGoneStyle = _settings.GetStyle("stat-lives-lost", "stat-mines-bg");

        string atext = " ";
        for (int i = 0; i < state.Difficulty.Lives - state.RoundState.LivesLost; i++) 
            atext += life + " ";

        string btext = "";
        for (int i = 0; i < state.RoundState.LivesLost; i++) 
            btext += life + " ";

        _renderer.PrintAtTile(minePosition, livesLeftStyle, atext);
        _renderer.PrintAtTile(minePosition.Shifted(atext.Length, 0), livesGoneStyle, btext);
    }
}
