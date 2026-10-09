// Stream 2 — the Reversi family's piece product. Every Reversi placement is
// a double-sided Disk; flipping changes its owner rather than replacing it.
namespace BoardGames.Core.Model.Pieces;

public class ReversiPieceFactory : IPieceFactory
{
    public Piece CreatePiece(Move move, PlayerSide owner)
    {
        return new Disk(owner);
    }
}
