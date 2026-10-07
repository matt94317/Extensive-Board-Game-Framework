// Stream 1 — an ordinary Gomoku stone: X for Player 1, O for Player 2.
// Ordinary stones are the only pieces an Eraser stone may remove.
namespace BoardGames.Core.Model.Pieces;

public class Stone : Piece
{
    public Stone(PlayerSide owner) : base(owner) { }

    public override char Symbol
    {
        get { return Owner == PlayerSide.PlayerOne ? 'X' : 'O'; }
    }

    public override bool CanBeErased
    {
        get { return true; }
    }
}
