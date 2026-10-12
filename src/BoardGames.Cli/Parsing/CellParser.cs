// Stream 4 — the "[row]:[col]" part shared by every move notation in the
// brief (O5:3, H3:3, P3:4, ...). Only checks the shape and that both parts
// are numbers; whether the cell is on the board is the rules' job, so the
// player gets the variant's own "outside the 8x8 board" message.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing;

internal static class CellParser
{
    public static Position Parse(string cellText, string token, string syntaxHint)
    {
        string[] parts = cellText.Split(':');
        if (parts.Length != 2
            || !int.TryParse(parts[0].Trim(), out int row)
            || !int.TryParse(parts[1].Trim(), out int col))
            throw new InvalidMoveException("'" + token + "' is not a valid move. " + syntaxHint);
        return new Position(row, col);
    }
}
