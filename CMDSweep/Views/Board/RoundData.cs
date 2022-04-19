using CMDSweep.Geometry;
using CMDSweep.IO;
using System;

namespace CMDSweep.Views.Board;

internal record struct RoundData
{
    public readonly Difficulty Difficulty;
    public readonly PlayerState PlayerState;
    public readonly Point Cursor;
    public readonly int Lives;
    public readonly bool Highscore;
    public Face Face;

    public RoundData(Difficulty difficulty, PlayerState playerState, Point cursor, int lives, bool highscore, Face face)
    {
        Difficulty = difficulty;
        PlayerState = playerState;
        Cursor = cursor;
        Lives = lives;
        Highscore = highscore;
        Face = face;
    }

    public static RoundData NewGame(Difficulty diff)
    {
        return new RoundData(
            diff,
            PlayerState.NewGame,
            new Rectangle(0, 0, diff.Width, diff.Height).Center,
            diff.Lives,
            false,
            Face.Normal
        );
    }

    public int Mines => Difficulty.Mines;

    public int LivesLost => Difficulty.Lives - Lives;

    public bool CanLoseLife => Lives > 1;

    internal RoundData Win() => new (Difficulty, PlayerState.Win, Cursor, Lives, Highscore, Face.Win);

    internal RoundData LoseLife() => new (Difficulty, PlayerState, Cursor, Lives, Highscore, Face);

    internal RoundData Die() => new (Difficulty, PlayerState.Dead, Cursor, 0, Highscore, Face.Dead);

    internal RoundData SetCursor(Point p) => new (Difficulty, PlayerState, p, Lives, Highscore, Face);

    internal RoundData MoveCursor(Direction d)
    {
        return d switch
        {
            Direction.Down => SetCursor(new Point(Cursor.X, Cursor.Y + 1)),
            Direction.Up => SetCursor(new Point(Cursor.X, Cursor.Y - 1)),
            Direction.Left => SetCursor(new Point(Cursor.X - 1, Cursor.Y)),
            Direction.Right => SetCursor(new Point(Cursor.X + 1, Cursor.Y)),
            _ => this,
        };
    }

    internal RoundData SetState(PlayerState state) => new(Difficulty, state, Cursor, Lives, Highscore, Face);
}
