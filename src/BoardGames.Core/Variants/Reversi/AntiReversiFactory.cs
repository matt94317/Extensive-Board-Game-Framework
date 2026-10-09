// Stream 2 — Anti-Reversi (misère): same play as Standard, but the factory
// swaps in the fewest-disks win condition.
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Variants.Reversi;

public class AntiReversiFactory : ReversiFactory
{
    public override string VariantKey
    {
        get { return "anti"; }
    }

    protected override string VariantName
    {
        get { return "Anti-Reversi"; }
    }

    protected override string WinRule
    {
        get { return "FEWEST disks at the end wins (misere); equal counts are a draw."; }
    }

    public override IWinCondition CreateWinCondition()
    {
        return new FewestDisksCondition();
    }

    public override IMoveStrategy CreateSmartStrategy()
    {
        return new AntiReversiStrategy();
    }
}
