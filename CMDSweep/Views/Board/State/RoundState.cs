using CMDSweep.Data;

namespace CMDSweep.Views.Board.State;

internal record struct RoundState
{
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly int Lives;
    public readonly bool Highscore;
    public Face Face;

    public RoundState(Difficulty difficulty, PlayerState playerState, int lives, bool highscore, Face face)
    {
        Difficulty = difficulty;
        PlayerState = playerState;
        Lives = lives;
        Highscore = highscore;
        Face = face;
    }

    public static RoundState NewGame(Difficulty diff)
    {
        return new RoundState(
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

    public RoundState Win() => new(Difficulty, PlayerState.Win, Lives, Highscore, Face.Win);

    public RoundState LoseLife() => new(Difficulty, PlayerState, Lives, Highscore, Face);

    public RoundState Die() => new(Difficulty, PlayerState.Dead, 0, Highscore, Face.Dead);

    public RoundState SetState(PlayerState state) => new(Difficulty, state, Lives, Highscore, Face);
}
