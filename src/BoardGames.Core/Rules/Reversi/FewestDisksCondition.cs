// Stream 2 — Anti-Reversi (misère): the fewest disks at the end wins; equal
// counts are a draw.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Rules.Reversi;

public class FewestDisksCondition : ReversiWinCondition
{
    protected override GameResult Decide(int playerOneDisks, int playerTwoDisks)
    {
        if (playerOneDisks < playerTwoDisks)
            return GameResult.Won(PlayerSide.PlayerOne);
        if (playerTwoDisks < playerOneDisks)
            return GameResult.Won(PlayerSide.PlayerTwo);
        return GameResult.Draw();
    }
}
