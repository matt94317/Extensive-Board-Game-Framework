// Streams 1 & 3 — COMMAND PATTERN contract. This interface is the seam
// between the engine (Stream 1), which executes and logs commands inside
// its fixed turn algorithm, and the history work (Stream 3), which stacks
// them for multi-turn undo/redo. Every command stores exactly the state it
// changes, so Undo() can reverse it precisely.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Commands;

public interface IGameCommand
{
    Move Move { get; }
    PlayerSide Actor { get; }

    void Execute(Game game);
    void Undo(Game game);
}
