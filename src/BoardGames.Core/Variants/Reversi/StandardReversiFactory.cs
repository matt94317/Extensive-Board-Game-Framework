// Stream 2 — Standard Reversi: most disks wins.
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Variants.Reversi;

public class StandardReversiFactory : ReversiFactory
{
    public override string VariantKey
    {
        get { return "standard"; }
    }

    protected override string VariantName
    {
        get { return "Standard Reversi"; }
    }

    protected override string WinRule
    {
        get { return "strictly more disks at the end wins; equal counts are a draw."; }
    }

    public override IWinCondition CreateWinCondition()
    {
        return new MostDisksCondition();
    }

    public override IMoveStrategy CreateSmartStrategy()
    {
        return new GreedyFlipStrategy();
    }
}
