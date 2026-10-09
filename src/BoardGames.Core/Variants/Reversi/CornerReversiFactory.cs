// Stream 2 — Corner Reversi: same play as Standard, but the factory swaps in
// the corner-dominance win condition (3 corners = instant win).
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Variants.Reversi;

public class CornerReversiFactory : ReversiFactory
{
    public override string VariantKey
    {
        get { return "corner"; }
    }

    protected override string VariantName
    {
        get { return "Corner Reversi"; }
    }

    protected override string WinRule
    {
        get
        {
            return "first to hold 3 of the 4 corners wins instantly, whatever the\n"
                 + "     disk counts; otherwise strictly more disks wins, equal is a draw.";
        }
    }

    public override IWinCondition CreateWinCondition()
    {
        return new CornerDominanceCondition();
    }

    public override IMoveStrategy CreateSmartStrategy()
    {
        return new CornerReversiStrategy();
    }
}
