using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Game.State;

internal record struct GameProgressState : IRenderState
{
    private readonly int _id;
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly int Lives;
    public readonly bool Highscore;
    public Face Face;

    public GameProgressState(Difficulty difficulty, PlayerState playerState, int lives, bool highscore, Face face, int id)
    {
        Difficulty = difficulty;
        PlayerState = playerState;
        Lives = lives;
        Highscore = highscore;
        Face = face;
        _id = id;
    }

    public static GameProgressState NewGame(Difficulty diff)
    {
        return new GameProgressState(
            diff,
            PlayerState.NewGame,
            diff.Lives,
            false,
            Face.Normal,
            0
        );
    }

    public int Id => _id;

    public int Mines => Difficulty.Mines;

    public int LivesLost => Difficulty.Lives - Lives;

    public bool CanLoseLife => Lives > 1;

    public bool Dead => PlayerState == PlayerState.Dead;

    public GameProgressState Win() => new(Difficulty, PlayerState.Win, Lives, Highscore, Face.Win, _id + 1);

    public GameProgressState LoseLife() => new(Difficulty, PlayerState, Lives, Highscore, Face, _id + 1);

    public GameProgressState Die() => new(Difficulty, PlayerState.Dead, 0, Highscore, Face.Dead, _id + 1);

    public GameProgressState SetState(PlayerState state) => new(Difficulty, state, Lives, Highscore, Face, _id + 1);
}
