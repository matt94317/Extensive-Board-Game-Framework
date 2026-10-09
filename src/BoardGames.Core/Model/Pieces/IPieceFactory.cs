// Stream 1 — Factory Method for pieces: each family supplies the factory
// that turns a validated move into the right kind of piece, so the engine
// and commands never name a concrete piece class.
namespace BoardGames.Core.Model.Pieces;

public interface IPieceFactory
{
    Piece CreatePiece(Move move, PlayerSide owner);
}
