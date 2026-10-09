// Stream 2 — Corner Reversi: holding 3 of the 4 corners at any point is an
// instant win, whatever the disk counts. A corner disk can never be
// flanked, so once taken it is held for good. If the game ends with no
// 3-corner holder it falls back to Standard's most-disks verdict.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Rules.Reversi;

public class CornerDominanceCondition : MostDisksCondition
{
    public const int CornersToWin = 3;

    protected override GameResult CheckSuddenDeath(Game game)
    {
        if (CornersHeld(game.Board, PlayerSide.PlayerOne) >= CornersToWin)
            return GameResult.Won(PlayerSide.PlayerOne);
        if (CornersHeld(game.Board, PlayerSide.PlayerTwo) >= CornersToWin)
            return GameResult.Won(PlayerSide.PlayerTwo);
        return GameResult.InProgress();
    }

    public static List<Position> Corners(Board board)
    {
        return new List<Position>
        {
            new Position(1, 1), new Position(1, board.Size),
            new Position(board.Size, 1), new Position(board.Size, board.Size)
        };
    }

    public static int CornersHeld(Board board, PlayerSide side)
    {
        int held = 0;
        foreach (Position corner in Corners(board))
        {
            Piece? piece = board.GetPiece(corner);
            if (piece != null && piece.Owner == side)
                held++;
        }
        return held;
    }
}
