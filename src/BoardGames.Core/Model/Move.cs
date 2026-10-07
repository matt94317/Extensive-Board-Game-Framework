// Stream 1 — a player's intention for one turn, before it is validated.
// The move syntax of the brief maps one-to-one onto MoveType:
//   O r:c -> Place      H r:c -> PlaceHeavy      E r:c -> Erase      PASS -> Pass
namespace BoardGames.Core.Model;

public enum MoveType
{
    Place,
    PlaceHeavy,
    Erase,
    Pass
}

public sealed class Move
{
    public MoveType Type { get; }
    public Position Position { get; }

    public Move(MoveType type, Position position)
    {
        Type = type;
        Position = position;
    }

    // PASS carries no coordinate; (0,0) is never a legal cell (1-based board).
    public static Move Pass()
    {
        return new Move(MoveType.Pass, new Position(0, 0));
    }
}
