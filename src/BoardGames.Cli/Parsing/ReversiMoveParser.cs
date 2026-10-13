// Stream 4 - Reversi family syntax: P[r]:[c] places a disk, PASS passes the
// turn. Whether a PASS is allowed is checked by ReversiMoveRules.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing
{
    public class ReversiMoveParser : IMoveParser
    {
        public Move Parse(string text)
        {
            string command = text.Trim().ToUpper();
            if (command == "PASS")
                return Move.Pass();
            if (command.Length < 2 || command[0] != 'P')
                throw new InvalidMoveException("'" + text + "' is not a valid move. Use P[r]:[c] or PASS.");
            return new Move(MoveType.Place, CellParser.ParseCell(command.Substring(1), text));
        }
    }
}
