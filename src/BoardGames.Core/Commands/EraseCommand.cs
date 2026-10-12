// Streams 1 & 3 — the GomokuPlus Eraser: removes one opponent ORDINARY
// stone and spends an Eraser from the actor's inventory. The removed piece
// is kept inside the command so Undo() puts the very same object back and
// refunds the Eraser — symmetric reversion, as the brief requires.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Commands;

public class EraseCommand : IGameCommand
{
    private readonly Inventory inventory;
    private Piece? removed;

    public Move Move { get; }
    public PlayerSide Actor { get; }

    public EraseCommand(Move move, PlayerSide actor, Inventory inventory)
    {
        Move = move;
        Actor = actor;
        this.inventory = inventory;
    }

    public void Execute(Game game)
    {
        inventory.UseEraser();
        removed = game.Board.Remove(Move.Position);
    }

    public void Undo(Game game)
    {
        game.Board.Place(Move.Position, removed!);
        inventory.RestoreEraser();
    }
}
