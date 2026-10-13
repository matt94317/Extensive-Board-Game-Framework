// Stream 4 — Gomoku notation: O[r]:[c] ordinary stone, H[r]:[c] Heavy
// stone, E[r]:[c] Eraser (letters are case-insensitive). H and E parse in
// every Gomoku variant; Standard and Fog then reject them through
// GomokuMoveRules, so the "special stones exist only in GomokuPlus" message
// comes from the rules, not from here. PASS is handed on the same way.
// The syntax hint only advertises H and E for the variant that has them.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli.Parsing;

public class GomokuMoveParser : IMoveParser
{
    private readonly bool specialStones;

    public GomokuMoveParser(bool specialStones = false)
    {
        this.specialStones = specialStones;
    }

    public string SyntaxHint
    {
        get
        {
            if (specialStones)
                return "Use O[row]:[col], H[row]:[col] or E[row]:[col], e.g. O5:3.";
            return "Use O[row]:[col], e.g. O5:3.";
        }
    }

    public Move Parse(string token)
    {
        string text = token.Trim().ToUpperInvariant();
        if (text == "PASS")
            return Move.Pass();
        if (text.Length < 2)
            throw new InvalidMoveException("'" + token + "' is not a valid move. " + SyntaxHint);

        MoveType type;
        switch (text[0])
        {
            case 'O': type = MoveType.Place; break;
            case 'H': type = MoveType.PlaceHeavy; break;
            case 'E': type = MoveType.Erase; break;
            default:
                throw new InvalidMoveException("'" + token + "' is not a valid move. " + SyntaxHint);
        }
        return new Move(type, CellParser.Parse(text.Substring(1), token, SyntaxHint));
    }
}
