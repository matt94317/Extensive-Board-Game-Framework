// Streams 1 & 3 — places a stone (ordinary or Heavy) on an empty cell.
// Implemented alongside the engine because Gomoku placement is Stream 1's
// vertical slice; Stream 3 owns the remaining commands and the undo/redo
// stacks that replay these.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Commands;

public class PlaceCommand : IGameCommand
{
    private readonly Piece piece;
    private readonly Inventory? heavyInventory;   // set only for Heavy Stones

    public Move Move { get; }
    public PlayerSide Actor { get; }

    public PlaceCommand(Move move, PlayerSide actor, Piece piece, Inventory? heavyInventory)
    {
        Move = move;
        Actor = actor;
        this.piece = piece;
        this.heavyInventory = heavyInventory;
    }

    public void Execute(Game game)
    {
        if (heavyInventory != null)
            heavyInventory.UseHeavy();
        game.Board.Place(Move.Position, piece);
    }

    public void Undo(Game game)
    {
        game.Board.Remove(Move.Position);
        if (heavyInventory != null)
            heavyInventory.RestoreHeavy();
    }
}
