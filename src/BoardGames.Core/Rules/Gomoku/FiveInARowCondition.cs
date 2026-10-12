// Stream 1 — the Gomoku win strategy: five or more of the mover's pieces in
// an unbroken horizontal, vertical or diagonal line. Evaluated while the
// mover is still the active side (see Game.Play), so only that side needs
// checking; a full board with no winner is a draw.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Rules.Gomoku;

public class FiveInARowCondition : IWinCondition
{
    public const int WinLength = 5;

    // Right, down, down-right, down-left: each line is found from its
    // start, so four directions cover all eight.
    private static readonly int[,] Directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

    public GameResult Evaluate(Game game)
    {
        if (HasRun(game.Board, game.ActiveSide))
            return GameResult.Won(game.ActiveSide);
        if (game.Board.IsFull())
            return GameResult.Draw();
        return GameResult.InProgress();
    }

    private bool HasRun(Board board, PlayerSide side)
    {
        for (int row = 1; row <= board.Size; row++)
            for (int col = 1; col <= board.Size; col++)
                for (int d = 0; d < 4; d++)
                    if (RunLengthFrom(board, side, row, col,
                                      Directions[d, 0], Directions[d, 1]) >= WinLength)
                        return true;
        return false;
    }

    private int RunLengthFrom(Board board, PlayerSide side,
                              int row, int col, int deltaRow, int deltaCol)
    {
        int length = 0;
        Position position = new Position(row, col);
        while (board.IsInside(position))
        {
            Piece? piece = board.GetPiece(position);
            if (piece == null || piece.Owner != side)
                break;
            length++;
            position = new Position(position.Row + deltaRow, position.Column + deltaCol);
        }
        return length;
    }
}
