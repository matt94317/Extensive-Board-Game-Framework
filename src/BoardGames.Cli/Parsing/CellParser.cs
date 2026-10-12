// Stream 4 - reads the "[row]:[col]" part that every move command shares
// (O5:3, H3:3, E3:3, P3:4). Only the format is checked here; whether the cell
// is on the board is checked by the variant's rules.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing
{
    public static class CellParser
    {
        public static Position ParseCell(string cellText, string originalText)
        {
            string[] parts = cellText.Split(':');
            int row;
            int col;
            if (parts.Length != 2 || !int.TryParse(parts[0], out row) || !int.TryParse(parts[1], out col))
                throw new InvalidMoveException("'" + originalText + "' is not a valid command. Use [row]:[col], e.g. 5:3.");
            return new Position(row, col);
        }
    }
}
