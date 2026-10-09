// Stream 2 — Smarter AI for all three Gomoku variants. For every empty cell
// it measures the longest line it would complete for itself and for the
// opponent, then plays, in priority order:
//   1. an immediate five of its own (win)
//   2. the cell that stops the opponent's five (blocks a four, gapped or not)
//   3. the cell that best extends its own lines while cutting the
//      opponent's, nearer the centre on ties
// Lines are counted only from stones the AI can see through the variant's
// perspective, so in GomokuFog it plays fair and cannot react to stones
// hidden by the fog; Standard and Plus see the whole board.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Players.Strategies;

public class GomokuSmartStrategy : IMoveStrategy
{
    public const int WinLength = 5;

    private const int WinsNow = 1000000;
    private const int BlocksWin = 100000;

    // Right, down, down-right, down-left: each axis is walked both ways.
    private static readonly int[,] Axes = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

    public Move ChooseMove(Game game, PlayerSide side)
    {
        List<Move> legal = game.Rules.LegalMoves(game, side);
        if (legal.Count == 0)
            return Move.Pass();   // board full: the game is already over

        PlayerSide opponent = PlayerSides.Opponent(side);
        Move best = legal[0];
        int bestScore = int.MinValue;
        foreach (Move move in legal)
        {
            int own = LongestLine(game, side, side, move.Position);
            int theirs = LongestLine(game, opponent, side, move.Position);

            int score;
            if (own >= WinLength)
                score = WinsNow;
            else if (theirs >= WinLength)
                score = BlocksWin;
            else
                score = own * own * 10 + theirs * theirs * 8
                      - DistanceFromCentre(game.Board, move.Position);

            if (score > bestScore)
            {
                best = move;
                bestScore = score;
            }
        }
        return best;
    }

    // The longest straight line 'owner' would have through 'cell' if a
    // stone of theirs were placed there, counting only stones 'viewer' sees.
    private int LongestLine(Game game, PlayerSide owner, PlayerSide viewer, Position cell)
    {
        int longest = 0;
        for (int a = 0; a < 4; a++)
        {
            int length = 1
                + CountRun(game, owner, viewer, cell, Axes[a, 0], Axes[a, 1])
                + CountRun(game, owner, viewer, cell, -Axes[a, 0], -Axes[a, 1]);
            if (length > longest)
                longest = length;
        }
        return longest;
    }

    private int CountRun(Game game, PlayerSide owner, PlayerSide viewer, Position cell,
                         int deltaRow, int deltaCol)
    {
        int count = 0;
        Position current = new Position(cell.Row + deltaRow, cell.Column + deltaCol);
        while (true)
        {
            Piece? piece = game.Board.GetPiece(current);
            if (piece == null || piece.Owner != owner
                || !game.Perspective.IsVisible(game, viewer, current))
                return count;
            count++;
            current = new Position(current.Row + deltaRow, current.Column + deltaCol);
        }
    }

    private int DistanceFromCentre(Board board, Position position)
    {
        int centre = (board.Size + 1) / 2;
        return Math.Abs(position.Row - centre) + Math.Abs(position.Column - centre);
    }
}
