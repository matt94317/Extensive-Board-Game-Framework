// Stream 4 - turns the text a player types (or a test script contains) into a
// Core Move. Each game family has its own syntax, so each family has its own
// parser class; the game loop and the CLI test mode only use this interface.
// A parser checks SYNTAX only - whether the move is legal is decided by the
// variant's IMoveRules when Game.Play() is called.
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing
{
    public interface IMoveParser
    {
        // Throws InvalidMoveException when the text is not a valid command.
        Move Parse(string text);
    }
}
