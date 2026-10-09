// Streams 2 & 3 — a Reversi PASS. It changes nothing on the board, but it
// is still a command so the turn is logged, counted and undoable like any
// other move.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Commands;

public class PassCommand : IGameCommand
{
    public Move Move { get; }
    public PlayerSide Actor { get; }

    public PassCommand(PlayerSide actor)
    {
        Move = Move.Pass();
        Actor = actor;
    }

    public void Execute(Game game) { }

    public void Undo(Game game) { }
}
