using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Game;

internal class BoardVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly TileVisualizer _tileVisualizer;
    private readonly IRenderer _renderer;

    public BoardVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _tileVisualizer = new TileVisualizer(renderer, settings);
    }

    public void Visualize(GameState gameState)
    {
        Rectangle viewport = gameState.BoardState.View.ViewPort;

        if (viewport.Area == 0)
            throw new System.Exception("The visualized area is empty.");

        foreach (Point p in viewport)
            _tileVisualizer.Visualize(p, gameState);
    }

    public void VisualizeChanges(GameState gameState, GameState previousGameState)
    {
        if ((gameState.Dead) != (previousGameState.Dead))
        {
            Visualize(gameState);
            return;
        }

        List<Point> changes;
        changes = gameState.BoardState.FindChangedTiles(previousGameState.BoardState);

        if (gameState.BoardState.View != previousGameState.BoardState.View)
        {
            List<Point> NewlyVisibleTiles = RenderScroll(gameState.BoardState.View, previousGameState.BoardState.View);
            changes.AddRange(NewlyVisibleTiles);
        }

        foreach (Point p in changes)
        {
            _tileVisualizer.Visualize(p, gameState);
        }
    }

    private List<Point> RenderScroll(BoardViewState newView, BoardViewState oldView)
    {
        // TODO: Does not work, needs to be debugged and tested. It looks like the mapping function does not work as expected or it is used wrong

        Rectangle copyableArea = newView.ViewPort.Intersect(oldView.ViewPort);
        RenderBufferCopyTask task = new RenderBufferCopyTask(oldView.MapToRender(copyableArea), newView.MapToRender(copyableArea));
        _renderer.CopyArea(task);

        List<Point> newpoints = new(newView.ViewPort);
        return newpoints.FindAll(p => !copyableArea.Contains(p));
    }
}
