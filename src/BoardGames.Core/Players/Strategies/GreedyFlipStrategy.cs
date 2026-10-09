// Stream 2 — Smarter AI for Standard Reversi: greedy, maximising the disks
// flipped this turn. Between moves that flip the same number, a corner wins
// because a corner disk can never be flipped back.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Players.Strategies;

public class GreedyFlipStrategy : ReversiScoringStrategy
{
    protected override int Score(Game game, PlayerSide side, Move move, List<Position> flips)
    {
        int score = flips.Count * 10;
        if (IsCorner(game.Board, move.Position))
            score += 5;
        return score;
    }
}
