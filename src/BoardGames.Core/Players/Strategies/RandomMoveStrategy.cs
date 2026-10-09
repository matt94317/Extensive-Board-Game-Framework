// Stream 2 — the Dumb AI for all six variants: any legal move, chosen at
// random; PASS when there is none. The Random is injected so tests can
// seed it and get the same game every run.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Players.Strategies;

public class RandomMoveStrategy : IMoveStrategy
{
    private readonly Random random;

    public RandomMoveStrategy(Random random)
    {
        this.random = random;
    }

    public Move ChooseMove(Game game, PlayerSide side)
    {
        List<Move> legal = game.Rules.LegalMoves(game, side);
        if (legal.Count == 0)
            return Move.Pass();
        return legal[random.Next(legal.Count)];
    }
}
