// Stream 1 — the outcome of a win-condition evaluation: the state plus, when
// the game is won, which side won it. Immutable; created via the static
// factory methods so call sites read naturally.
using BoardGames.Core.Model;

namespace BoardGames.Core.Engine;

public sealed class GameResult
{
    public GameState State { get; }
    public PlayerSide Winner { get; }

    private GameResult(GameState state, PlayerSide winner)
    {
        State = state;
        Winner = winner;
    }

    public static GameResult InProgress()
    {
        return new GameResult(GameState.InProgress, PlayerSide.None);
    }

    public static GameResult Won(PlayerSide winner)
    {
        return new GameResult(GameState.Won, winner);
    }

    public static GameResult Draw()
    {
        return new GameResult(GameState.Draw, PlayerSide.None);
    }
}
