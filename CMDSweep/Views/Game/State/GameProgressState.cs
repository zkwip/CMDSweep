using CMDSweep.Data;

namespace CMDSweep.Views.Game.State;

internal record struct GameProgressState
{
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly int Lives;
    public readonly bool Highscore;
    public Face Face;

    public GameProgressState(Difficulty difficulty, PlayerState playerState, int lives, bool highscore, Face face)
    {
        Difficulty = difficulty;
        PlayerState = playerState;
        Lives = lives;
        Highscore = highscore;
        Face = face;
    }

    public static GameProgressState NewGame(Difficulty diff)
    {
        return new GameProgressState(
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

    public GameProgressState Win() => new(Difficulty, PlayerState.Win, Lives, Highscore, Face.Win);

    public GameProgressState LoseLife() => new(Difficulty, PlayerState, Lives, Highscore, Face);

    public GameProgressState Die() => new(Difficulty, PlayerState.Dead, 0, Highscore, Face.Dead);

    public GameProgressState SetState(PlayerState state) => new(Difficulty, state, Lives, Highscore, Face);
}
