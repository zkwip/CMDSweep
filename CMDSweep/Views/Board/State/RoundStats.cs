using CMDSweep.Geometry;
using CMDSweep.IO;
using System;

namespace CMDSweep.Views.Board.State;

internal record struct RoundStats
{
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly int Lives;
    public readonly bool Highscore;
    public Face Face;

    public RoundStats(Difficulty difficulty, PlayerState playerState, int lives, bool highscore, Face face)
    {
        Difficulty = difficulty;
        PlayerState = playerState;
        Lives = lives;
        Highscore = highscore;
        Face = face;
    }

    public static RoundStats NewGame(Difficulty diff)
    {
        return new RoundStats(
            diff,
            PlayerState.NewGame,
            diff.Lives,
            false,
            Face.Normal
        );
    }

    public int Mines => Difficulty.Mines;

    public int LivesLost => Difficulty.Lives - Lives;

    public bool CanLoseLife => Lives > 1;

    public RoundStats Win() => new(Difficulty, PlayerState.Win, Lives, Highscore, Face.Win);

    public RoundStats LoseLife() => new(Difficulty, PlayerState, Lives, Highscore, Face);

    public RoundStats Die() => new(Difficulty, PlayerState.Dead, 0, Highscore, Face.Dead);

    public RoundStats SetState(PlayerState state) => new(Difficulty, state, Lives, Highscore, Face);
}
