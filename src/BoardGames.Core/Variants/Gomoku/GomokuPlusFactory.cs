// Stream 1 — GomokuPlus: Heavy and Eraser stones enabled (2 of each per
// player). The only change is the flag this factory sets; rules, win
// condition and perspective are inherited.
namespace BoardGames.Core.Variants.Gomoku;

public class GomokuPlusFactory : GomokuFactory
{
    public override string VariantKey
    {
        get { return "plus"; }
    }

    protected override string VariantName
    {
        get { return "GomokuPlus"; }
    }

    protected override bool SpecialStonesEnabled
    {
        get { return true; }
    }
}
