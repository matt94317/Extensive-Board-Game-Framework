// Stream 1 — an immutable board coordinate. Rows and columns are 1-based,
// exactly as the assignment brief writes moves (e.g. O5:3 means row 5, col 3).
namespace BoardGames.Core.Model;

public sealed class Position
{
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override bool Equals(object? obj)
    {
        return obj is Position other && other.Row == Row && other.Column == Column;
    }

    public override int GetHashCode()
    {
        return Row * 100 + Column;
    }

    public override string ToString()
    {
        return Row + ":" + Column;
    }
}
