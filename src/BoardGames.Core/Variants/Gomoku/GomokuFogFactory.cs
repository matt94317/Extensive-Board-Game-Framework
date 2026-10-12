// Stream 1 — GomokuFog: identical rules to Standard; the factory swaps the
// perspective product so views mask cells the player has not revealed.
// Hidden information is a view policy, not a rules change.
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Gomoku;

namespace BoardGames.Core.Variants.Gomoku;

public class GomokuFogFactory : GomokuFactory
{
    public override string VariantKey
    {
        get { return "fog"; }
    }

    protected override string VariantName
    {
        get { return "GomokuFog"; }
    }

    protected override bool HiddenInformation
    {
        get { return true; }
    }

    public override IPerspective CreatePerspective()
    {
        return new FogPerspective();
    }
}
