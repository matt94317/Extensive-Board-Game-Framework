// Stream 4 — turns the text a player types (or a test script contains) into
// a Core Move. Each family has its own syntax (Gomoku O/H/E r:c, Reversi
// P r:c / PASS), so each supplies its own parser; the game loop and the CLI
// test mode only ever talk to this interface. Parsing checks SYNTAX only -
// whether the move is legal is still decided by the variant's IMoveRules.
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing;

public interface IMoveParser
{
    // Throws InvalidMoveException, naming the expected syntax, when 'token'
    // is not a move in this family's notation.
    Move Parse(string token);

    // One-line reminder of the syntax, shown after a parse error.
    string SyntaxHint { get; }
}
