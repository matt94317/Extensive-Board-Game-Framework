// Stream 1 — base class of everything that can sit on a cell. A piece knows
// who owns it, how it is drawn, and whether an Eraser may remove it; the
// subclasses specialise exactly those three things and nothing else.
namespace BoardGames.Core.Model.Pieces;

public abstract class Piece
{
    public PlayerSide Owner { get; protected set; }

    protected Piece(PlayerSide owner)
    {
        Owner = owner;
    }

    public abstract char Symbol { get; }

    public virtual bool CanBeErased
    {
        get { return false; }
    }
}
