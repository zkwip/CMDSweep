using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Highscore;

class HighscoreVisualizer : Visualizer<Difficulty>
{
    TableGrid ScoreTable;
    public HighscoreVisualizer(HighscoreController hctrl) : base(hctrl)
    {
        HideStyle = Settings.GetStyle("menu");
        Resize();
    }

    public override bool CheckFullRefresh() => true;

    public override bool CheckResize() => true;

    public override bool CheckScroll() => false;

    public override void RenderChanges() => RenderFull();

    public override void RenderFull()
    {
        Renderer.ClearScreen(HideStyle);
        HighscoreTable.RenderHSTable(Renderer, Settings, ScoreTable, CurrentState!, HideStyle);
    }

    public override void Resize()
    {
        ScoreTable = HighscoreTable.GetHSTableGrid(Settings);
        ScoreTable.CenterOn(Renderer.Bounds.Center);
    }

    public override Difficulty RetrieveState() => ((HighscoreController)Controller).SelectedDifficulty;

    public override void Scroll() { }
}
