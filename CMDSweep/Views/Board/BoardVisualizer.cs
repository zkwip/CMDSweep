using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Board.State;
using System.Collections.Generic;

namespace CMDSweep.Views.Board;

partial class BoardVisualizer : Visualizer<BoardState>
{
    TileVisualizer _tileVisualizer;
    StatboardVisualizer _statboardVisualizer;
    public BoardVisualizer(BoardController bctrl) : base(bctrl)
    {
        _tileVisualizer = new TileVisualizer();
        _statboardVisualizer = new StatboardVisualizer(Renderer, Settings);
    }

    internal override BoardState RetrieveState() => ((BoardController)Controller).CurrentState;
    
    internal override bool CheckFullRefresh() => CurrentState!.RoundState.PlayerState != LastState!.RoundState.PlayerState;

    internal override bool CheckScroll() => !CurrentState!.ScrollIsNeeded;

    internal override void Scroll()
    {
        RenderBufferCopyTask task = CurrentState!.Scroll();
        Renderer.CopyArea(task);

        CurrentState!.View.Viewport.ForAll(p => { 
            if (!CurrentState!.View.IsScrollValid(p)) 
                RenderCell(p); 
            }
        );
    }

    private void RenderHighscorePopup(BoardState curGS)
    {
        TableGrid tableGrid = Highscores.GetHSTableGrid(Settings);
        tableGrid.CenterOn(Renderer.Bounds.Center);

        RenderPopupBox(Settings.GetStyle("popup"), tableGrid.Bounds.Grow(2), "popup-border");
        Highscores.RenderHSTable(Renderer, Settings, tableGrid, curGS.Difficulty, Settings.GetStyle("popup"));
    }

    internal override void RenderChanges()
    {
        List<Point> changes;
        changes = CurrentState!.CompareForVisibleChanges(LastState!);
        foreach (Point cl in changes) RenderCell(cl);
        _statboardVisualizer.Visualize(CurrentState!);
    }

    

    internal override bool CheckResize()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension();

        // Return if the measurements did not change
        return !newRenderMask.Equals(CurrentState!.View.RenderMask);
    }

    internal override void Resize()
    {
        Rectangle newRenderMask = RenderMaskFromConsoleDimension(); // Area that the board can be drawn into
        CurrentState!.View.ChangeRenderMask(newRenderMask);
    }

    private Rectangle RenderMaskFromConsoleDimension()
    {
        int barheight = 1 + 2 * Settings.Dimensions["stat-padding-y"];

        Rectangle newRenderMask = Renderer.Bounds.Shrink(0, barheight, 0, 0);
        return newRenderMask;
    }

    internal override void RenderFull()
    {
        CurrentState!.View.TryCenterViewPort();

        // Border
        Renderer.ClearScreen(HideStyle);
        CurrentState!.View.VisibleBoardSection.ForAll(p => RenderCell(p));

        // Extras
        RenderStatBoard();
        RenderMessages();

        Renderer.HideCursor(HideStyle);
        CurrentState!.View.ValidateViewPort();
    }

    private void RenderMessages()
    {
        if (CurrentState!.RoundState.PlayerState == PlayerState.Win)
            RenderPopup("Congratulations, You won!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundState.PlayerState == PlayerState.Dead)
            RenderPopup("You died!\n\nYou can play again by pressing any key.");

        if (CurrentState!.RoundState.PlayerState == PlayerState.ShowingHighscores)
            RenderHighscorePopup(CurrentState);

        if (CurrentState.RoundState.PlayerState == PlayerState.EnteringHighscore)
            RenderNewHighscorePopup();
    }

    private void RenderNewHighscorePopup()
    {
        BoardController bc = (BoardController)Controller;
        TextEnterField tef = bc.HighscoreTextField;

        TableGrid tg = new();

        tg.AddColumn(Settings.Dimensions["popup-enter-hs-width"], 0);
        tg.AddRow(2, 0);
        tg.AddRow(1, 0);
        tg.FitAround(0);

        Rectangle shape = tg.Bounds;
        RenderPopupAroundShape(shape);
        tg.Bounds = shape;

        tef.Bounds = tg.GetCell(0, 1);
        TextRenderBox trb = new TextRenderBox(Settings.Texts["popup-enter-hs-message"], tg.GetCell(0, 0));
        trb.Render(Renderer, Settings.GetStyle("popup"), false);

    }

    private void RenderCell(Point p) => _tileVisualizer.Visualize(p);
}
