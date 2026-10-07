// Stream 1 — piece factory for the Gomoku family: ordinary stones for
// Place, Heavy Stones for PlaceHeavy. (The Reversi family's factory, which
// produces disks, belongs to Stream 2.)
namespace BoardGames.Core.Model.Pieces;

public class GomokuPieceFactory : IPieceFactory
{
    public Piece CreatePiece(Move move, PlayerSide owner)
    {
        if (move.Type == MoveType.PlaceHeavy)
            return new HeavyStone(owner);
        return new Stone(owner);
    }
}
