// Stream 2 — Smarter AI for Anti-Reversi (fewest disks wins): minimise the
// disks flipped this turn and stay off the corners. A corner disk can never
// be flipped back and anchors whole lines of the AI's colour, which forces
// mass flips onto it later — exactly what loses a misère game. Edges carry
// a smaller penalty for the same reason.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Players.Strategies;

public class AntiReversiStrategy : ReversiScoringStrategy
{
    public const int CornerPenalty = 100;
    public const int EdgePenalty = 3;

    protected override int Score(Game game, PlayerSide side, Move move, List<Position> flips)
    {
        int cost = flips.Count * 10;
        if (IsCorner(game.Board, move.Position))
            cost += CornerPenalty * 10;
        else if (IsEdge(game.Board, move.Position))
            cost += EdgePenalty * 10;
        return -cost;
    }
}
