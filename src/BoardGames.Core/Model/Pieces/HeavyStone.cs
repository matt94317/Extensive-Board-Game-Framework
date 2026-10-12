// Stream 1 — a GomokuPlus Heavy Stone: drawn @ (P1) or # (P2) and immune to
// the Eraser. Inherits Stone because it behaves as a stone everywhere else
// (alignment counting treats both kinds identically).
namespace BoardGames.Core.Model.Pieces;

public class HeavyStone : Stone
{
    public HeavyStone(PlayerSide owner) : base(owner) { }

    public override char Symbol
    {
        get { return Owner == PlayerSide.PlayerOne ? '@' : '#'; }
    }

    public override bool CanBeErased
    {
        get { return false; }
    }
}
