﻿using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System.Collections.Generic;

namespace CMDSweep.Views.Game;

internal class BoardVisualizer : IChangeableTypeVisualizer<BoardState>
{
    private readonly TileVisualizer _tileVisualizer;

    public BoardVisualizer(IRenderer renderer, GameSettings settings, BoardState initialState)
    {
        _tileVisualizer = new TileVisualizer(renderer, settings, initialState);
    }

    public void Visualize(BoardState state)
    {
        foreach (Point p in state.View.ViewPort)
        {
            _tileVisualizer.Visualize(p);
        }
    }

    public void VisualizeChanges(BoardState state, BoardState previousState)
    {
        List<Point> changes;
        changes = state.FindChangedTiles(previousState);
        _tileVisualizer.UpdateBoardState(state);

        foreach (Point p in changes)
        {
            _tileVisualizer.Visualize(p);
        }
    }
}