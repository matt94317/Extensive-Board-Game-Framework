// Stream 1 — a double-sided Reversi disk. Flipping does not replace the
// object: the same disk changes owner, which is what makes undo symmetric
// (Stream 3 commands simply flip it back).
namespace BoardGames.Core.Model.Pieces;

public class Disk : Piece
{
    public Disk(PlayerSide owner) : base(owner) { }

    public override char Symbol
    {
        get { return Owner == PlayerSide.PlayerOne ? 'X' : 'O'; }
    }

    public void FlipTo(PlayerSide newOwner)
    {
        Owner = newOwner;
    }
}
