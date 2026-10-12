// Streams 1 & 2 — STRATEGY-style rules contract. One implementation per
// family encapsulates what a legal move is and which command realises it;
// the engine calls only this interface, which is how six variants share a
// single turn algorithm.
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Rules;

public interface IMoveRules
{
    // Throws InvalidMoveException with a clear reason when the move is illegal.
    void Validate(Game game, Move move);

    // Turns a validated move into the command object that performs it.
    IGameCommand CreateCommand(Game game, Move move);

    // Every currently legal move for the given side (used by the AIs and by
    // Reversi's PASS legality check).
    List<Move> LegalMoves(Game game, PlayerSide side);
}
