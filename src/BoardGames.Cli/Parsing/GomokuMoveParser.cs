// Stream 4 - Gomoku family syntax: O[r]:[c] ordinary stone, H[r]:[c] Heavy
// stone, E[r]:[c] Eraser. H and E are accepted by the parser in every Gomoku
// variant; Standard Gomoku and GomokuFog then reject them in their rules.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing
{
    public class GomokuMoveParser : IMoveParser
    {
        public Move Parse(string text)
        {
            string command = text.Trim().ToUpper();
            if (command == "PASS")
                return Move.Pass();   // the Gomoku rules will reject it
            if (command.Length < 2)
                throw new InvalidMoveException("'" + text + "' is not a valid move. Use O[r]:[c], H[r]:[c] or E[r]:[c].");

            MoveType type;
            switch (command[0])
            {
                case 'O':
                    type = MoveType.Place;
                    break;
                case 'H':
                    type = MoveType.PlaceHeavy;
                    break;
                case 'E':
                    type = MoveType.Erase;
                    break;
                default:
                    throw new InvalidMoveException("'" + text + "' is not a valid move. Use O[r]:[c], H[r]:[c] or E[r]:[c].");
            }
            return new Move(type, CellParser.ParseCell(command.Substring(1), text));
        }
    }
}
