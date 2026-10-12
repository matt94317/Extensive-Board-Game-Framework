// Stream 2 — shared skeleton of the three Smarter Reversi AIs. Choosing a
// move is always the same: no legal placement means PASS, otherwise score
// every legal placement and play the highest. Only the scoring differs
// between Standard, Anti and Corner, so that one step is left abstract.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Players.Strategies;

public abstract class ReversiScoringStrategy : IMoveStrategy
{
    protected readonly FlankingEngine Flanking = new FlankingEngine();

    public Move ChooseMove(Game game, PlayerSide side)
    {
        List<Move> legal = game.Rules.LegalMoves(game, side);
        if (legal.Count == 0)
            return Move.Pass();

        Move best = legal[0];
        int bestScore = int.MinValue;
        foreach (Move move in legal)
        {
            List<Position> flips = Flanking.FindFlips(game.Board, move.Position, side);
            int score = Score(game, side, move, flips);
            if (score > bestScore)
            {
                best = move;
                bestScore = score;
            }
        }
        return best;
    }

    // Higher is better. Ties go to the first move found (row by row).
    protected abstract int Score(Game game, PlayerSide side, Move move, List<Position> flips);

    protected static bool IsCorner(Board board, Position position)
    {
        return (position.Row == 1 || position.Row == board.Size)
            && (position.Column == 1 || position.Column == board.Size);
    }

    protected static bool IsEdge(Board board, Position position)
    {
        return position.Row == 1 || position.Row == board.Size
            || position.Column == 1 || position.Column == board.Size;
    }
}
