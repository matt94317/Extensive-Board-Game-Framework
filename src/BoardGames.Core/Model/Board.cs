// Stream 1 — the single authoritative grid. The board stores pieces and
// answers simple spatial questions; it knows no game rules at all, so the
// same class serves the 10x10 Gomoku family and the 8x8 Reversi family.
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Model;

public sealed class Board
{
    private readonly Piece?[,] cells;

    public int Size { get; }

    public Board(int size)
    {
        Size = size;
        cells = new Piece?[size + 1, size + 1];   // index 1..Size, row-major
    }

    public bool IsInside(Position position)
    {
        return position.Row >= 1 && position.Row <= Size
            && position.Column >= 1 && position.Column <= Size;
    }

    public bool IsEmpty(Position position)
    {
        return GetPiece(position) == null;
    }

    public Piece? GetPiece(Position position)
    {
        if (!IsInside(position))
            return null;
        return cells[position.Row, position.Column];
    }

    public void Place(Position position, Piece piece)
    {
        cells[position.Row, position.Column] = piece;
    }

    public Piece Remove(Position position)
    {
        Piece removed = cells[position.Row, position.Column]!;
        cells[position.Row, position.Column] = null;
        return removed;
    }

    public bool IsFull()
    {
        for (int row = 1; row <= Size; row++)
            for (int col = 1; col <= Size; col++)
                if (cells[row, col] == null)
                    return false;
        return true;
    }

    public int CountPieces(PlayerSide side)
    {
        int count = 0;
        for (int row = 1; row <= Size; row++)
            for (int col = 1; col <= Size; col++)
                if (cells[row, col] != null && cells[row, col]!.Owner == side)
                    count++;
        return count;
    }
}
