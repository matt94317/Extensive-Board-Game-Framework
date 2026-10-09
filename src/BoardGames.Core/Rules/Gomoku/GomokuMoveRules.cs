// Stream 1 — move legality for the whole Gomoku family. One instance serves
// Standard and Fog (special stones off) and GomokuPlus (special stones on):
// the variant difference is a constructor flag, not duplicated code.
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Variants.Gomoku;

namespace BoardGames.Core.Rules.Gomoku;

public class GomokuMoveRules : IMoveRules
{
    private readonly bool allowSpecialStones;

    public GomokuMoveRules(bool allowSpecialStones)
    {
        this.allowSpecialStones = allowSpecialStones;
    }

    public void Validate(Game game, Move move)
    {
        if (move.Type == MoveType.Pass)
            throw new InvalidMoveException("There is no passing in Gomoku.");

        if (!game.Board.IsInside(move.Position))
            throw new InvalidMoveException("Cell " + move.Position
                + " is outside the " + game.Board.Size + "x" + game.Board.Size + " board.");

        if (move.Type == MoveType.PlaceHeavy || move.Type == MoveType.Erase)
        {
            if (!allowSpecialStones)
                throw new InvalidMoveException(
                    "Heavy and Eraser stones exist only in GomokuPlus.");
            ValidateSpecial((GomokuGame)game, move);
            return;
        }

        if (!game.Board.IsEmpty(move.Position))
            throw new InvalidMoveException("Cell " + move.Position + " is already occupied.");
    }

    private void ValidateSpecial(GomokuGame game, Move move)
    {
        Inventory inventory = game.InventoryFor(game.ActiveSide);

        if (move.Type == MoveType.PlaceHeavy)
        {
            if (inventory.HeavyLeft <= 0)
                throw new InvalidMoveException("No Heavy Stones left.");
            if (!game.Board.IsEmpty(move.Position))
                throw new InvalidMoveException("Cell " + move.Position + " is already occupied.");
            return;
        }

        // Erase: must target an opponent piece that is erasable (ordinary).
        if (inventory.EraserLeft <= 0)
            throw new InvalidMoveException("No Eraser Stones left.");
        Piece? target = game.Board.GetPiece(move.Position);
        if (target == null)
            throw new InvalidMoveException("There is no stone at " + move.Position + " to erase.");
        if (target.Owner == game.ActiveSide)
            throw new InvalidMoveException("You cannot erase your own stone.");
        if (!target.CanBeErased)
            throw new InvalidMoveException("Heavy Stones cannot be erased.");
    }

    public IGameCommand CreateCommand(Game game, Move move)
    {
        GomokuGame gomoku = (GomokuGame)game;

        if (move.Type == MoveType.Erase)
            return new EraseCommand(move, game.ActiveSide,
                                    gomoku.InventoryFor(game.ActiveSide));

        Piece piece = gomoku.PieceFactory.CreatePiece(move, game.ActiveSide);
        Inventory? heavy = move.Type == MoveType.PlaceHeavy
            ? gomoku.InventoryFor(game.ActiveSide) : null;
        return new PlaceCommand(move, game.ActiveSide, piece, heavy);
    }

    public List<Move> LegalMoves(Game game, PlayerSide side)
    {
        // Ordinary placements only: that is all the Dumb AI needs, and the
        // Smarter AI (Stream 2) works from the same list.
        List<Move> moves = new List<Move>();
        for (int row = 1; row <= game.Board.Size; row++)
            for (int col = 1; col <= game.Board.Size; col++)
            {
                Position position = new Position(row, col);
                if (game.Board.IsEmpty(position))
                    moves.Add(new Move(MoveType.Place, position));
            }
        return moves;
    }
}
