using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;
using CMDSweep.Views.Game.State;
using System.Collections.Generic;

namespace CMDSweep.Views.Game;

internal class BoardVisualizer : IChangeableTypeVisualizer<GameState>
{
    private readonly TileVisualizer _tileVisualizer;

    public BoardVisualizer(IRenderer renderer, GameSettings settings)
    {
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

        foreach (Point p in changes)
        {
            _tileVisualizer.Visualize(p, gameState);
        }
    }
}
