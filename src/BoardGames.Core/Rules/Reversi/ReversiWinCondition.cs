// Stream 2 — shared skeleton of the three Reversi win strategies. When the
// game ends is the same for every variant (board full, or neither side has
// a legal move); only who wins differs. Evaluate() fixes that order as a
// small template method and leaves two steps to the variants:
//   CheckSuddenDeath — an instant win before the normal end (Corner only)
//   Decide           — the verdict from the final disk counts
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Rules.Reversi;

public abstract class ReversiWinCondition : IWinCondition
{
    public GameResult Evaluate(Game game)
    {
        GameResult early = CheckSuddenDeath(game);
        if (early.State != GameState.InProgress)
            return early;

        if (!IsOver(game))
            return GameResult.InProgress();

        return Decide(game.Board.CountPieces(PlayerSide.PlayerOne),
                      game.Board.CountPieces(PlayerSide.PlayerTwo));
    }

    // Checking both sides also settles PASS: if one player has just passed
    // and the other cannot move either, the game ends here.
    public static bool IsOver(Game game)
    {
        if (game.Board.IsFull())
            return true;
        return game.Rules.LegalMoves(game, PlayerSide.PlayerOne).Count == 0
            && game.Rules.LegalMoves(game, PlayerSide.PlayerTwo).Count == 0;
    }

    protected virtual GameResult CheckSuddenDeath(Game game)
    {
        return GameResult.InProgress();
    }

    protected abstract GameResult Decide(int playerOneDisks, int playerTwoDisks);
}
