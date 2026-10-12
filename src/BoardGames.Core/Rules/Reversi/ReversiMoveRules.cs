// Stream 2 — move legality for the whole Reversi family. All three variants
// share the same placement, flanking and PASS mechanics, so one instance
// serves Standard, Anti and Corner; only the win condition differs.
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Rules.Reversi;

public class ReversiMoveRules : IMoveRules
{
    private readonly FlankingEngine flanking;
    private readonly IPieceFactory pieceFactory;

    public ReversiMoveRules(FlankingEngine flanking, IPieceFactory pieceFactory)
    {
        this.flanking = flanking;
        this.pieceFactory = pieceFactory;
    }

    public void Validate(Game game, Move move)
    {
        if (move.Type == MoveType.Pass)
        {
            if (LegalMoves(game, game.ActiveSide).Count > 0)
                throw new InvalidMoveException(
                    "You have a legal move, so you cannot PASS.");
            return;
        }

        if (move.Type != MoveType.Place)
            throw new InvalidMoveException("Reversi moves are P[row]:[col] or PASS.");

        if (!game.Board.IsInside(move.Position))
            throw new InvalidMoveException("Cell " + move.Position
                + " is outside the " + game.Board.Size + "x" + game.Board.Size + " board.");

        if (!game.Board.IsEmpty(move.Position))
            throw new InvalidMoveException("Cell " + move.Position + " is already occupied.");

        if (flanking.FindFlips(game.Board, move.Position, game.ActiveSide).Count == 0)
            throw new InvalidMoveException("A disk at " + move.Position
                + " would not flip any opponent disks.");
    }

    public IGameCommand CreateCommand(Game game, Move move)
    {
        if (move.Type == MoveType.Pass)
            return new PassCommand(game.ActiveSide);

        Disk disk = (Disk)pieceFactory.CreatePiece(move, game.ActiveSide);
        List<Position> flips = flanking.FindFlips(game.Board, move.Position, game.ActiveSide);
        return new FlipPlaceCommand(move, game.ActiveSide, disk, flips);
    }

    // Placements only; an empty list means the side must PASS.
    public List<Move> LegalMoves(Game game, PlayerSide side)
    {
        List<Move> moves = new List<Move>();
        for (int row = 1; row <= game.Board.Size; row++)
            for (int col = 1; col <= game.Board.Size; col++)
            {
                Position position = new Position(row, col);
                if (flanking.FindFlips(game.Board, position, side).Count > 0)
                    moves.Add(new Move(MoveType.Place, position));
            }
        return moves;
    }
}
