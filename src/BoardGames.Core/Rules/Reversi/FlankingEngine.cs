// Stream 2 — the single flanking engine shared by all three Reversi
// variants. It answers one question without changing the board: which
// opponent disks would a placement flip? Move rules use it for legality,
// commands use its answer to flip, and the AIs use it to score moves.
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Rules.Reversi;

public class FlankingEngine
{
    // All eight straight-line directions: across, down and both diagonals.
    private static readonly int[,] Directions =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        {  0, -1 },            {  0, 1 },
        {  1, -1 }, {  1, 0 }, {  1, 1 }
    };

    // All opponent disks that placing 'side' at 'position' would flip.
    // Empty list = not a legal placement.
    public List<Position> FindFlips(Board board, Position position, PlayerSide side)
    {
        List<Position> flips = new List<Position>();
        if (!board.IsInside(position) || !board.IsEmpty(position))
            return flips;

        for (int d = 0; d < Directions.GetLength(0); d++)
            flips.AddRange(FlipsInDirection(board, position, side,
                                            Directions[d, 0], Directions[d, 1]));
        return flips;
    }

    // Walks outward over a continuous run of opponent disks; the run counts
    // only if it is closed off by one of the mover's own disks.
    private List<Position> FlipsInDirection(Board board, Position start, PlayerSide side,
                                            int deltaRow, int deltaCol)
    {
        List<Position> run = new List<Position>();
        PlayerSide opponent = PlayerSides.Opponent(side);
        Position current = new Position(start.Row + deltaRow, start.Column + deltaCol);

        while (true)
        {
            Piece? piece = board.GetPiece(current);   // null when empty or off the board
            if (piece == null)
                return new List<Position>();
            if (piece.Owner == side)
                return run;
            if (piece.Owner == opponent)
                run.Add(current);
            current = new Position(current.Row + deltaRow, current.Column + deltaCol);
        }
    }
}
