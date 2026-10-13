// Stream 4 — Reversi notation: P[r]:[c] places a disk, PASS passes (both
// case-insensitive). Whether a PASS is actually allowed is checked by
// ReversiMoveRules, which knows if the player has a legal placement.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing;

public class ReversiMoveParser : IMoveParser
{
    public string SyntaxHint
    {
        get { return "Use P[row]:[col] or PASS, e.g. P3:4."; }
    }

    public Move Parse(string token)
    {
        string text = token.Trim().ToUpperInvariant();
        if (text == "PASS")
            return Move.Pass();
        if (text.Length < 2 || text[0] != 'P')
            throw new InvalidMoveException("'" + token + "' is not a valid move. " + SyntaxHint);
        return new Move(MoveType.Place, CellParser.Parse(text.Substring(1), token, SyntaxHint));
    }
}
