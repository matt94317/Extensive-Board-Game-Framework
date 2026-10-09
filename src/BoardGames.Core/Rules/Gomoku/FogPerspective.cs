// Stream 1 — GomokuFog's hidden-information policy: a viewer sees their own
// stones and every cell adjacent (8 directions) to one of them; all other
// cells are hidden and rendered as '?' by the view. Only PRESENTATION is
// filtered — validation and win checking always run on the true board.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Rules.Gomoku;

public class FogPerspective : IPerspective
{
    public bool IsVisible(Game game, PlayerSide viewer, Position position)
    {
        Piece? here = game.Board.GetPiece(position);
        if (here != null && here.Owner == viewer)
            return true;

        for (int deltaRow = -1; deltaRow <= 1; deltaRow++)
            for (int deltaCol = -1; deltaCol <= 1; deltaCol++)
            {
                if (deltaRow == 0 && deltaCol == 0)
                    continue;
                Position neighbour = new Position(position.Row + deltaRow,
                                                  position.Column + deltaCol);
                Piece? piece = game.Board.GetPiece(neighbour);
                if (piece != null && piece.Owner == viewer)
                    return true;
            }
        return false;
    }
}
