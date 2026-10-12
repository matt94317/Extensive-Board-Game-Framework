// Stream 1 — the baseline variant: ordinary stones only, open information.
namespace BoardGames.Core.Variants.Gomoku;

public class StandardGomokuFactory : GomokuFactory
{
    public override string VariantKey
    {
        get { return "standard"; }
    }

    protected override string VariantName
    {
        get { return "Standard Gomoku"; }
    }
}
