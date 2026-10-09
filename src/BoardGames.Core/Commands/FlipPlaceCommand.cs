// Streams 2 & 3 — a Reversi placement: puts down one Disk and flips every
// flanked opponent disk. The flipped positions are worked out by the
// FlankingEngine before the command is built and stored here, so Undo()
// flips exactly those disks back and lifts the placed disk — symmetric
// reversion of the whole capture, as the brief requires.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;

namespace BoardGames.Core.Commands;

public class FlipPlaceCommand : IGameCommand
{
    private readonly Disk placed;
    private readonly List<Position> flipped;

    public Move Move { get; }
    public PlayerSide Actor { get; }

    public FlipPlaceCommand(Move move, PlayerSide actor, Disk placed, List<Position> flipped)
    {
        Move = move;
        Actor = actor;
        this.placed = placed;
        this.flipped = new List<Position>(flipped);
    }

    public IReadOnlyList<Position> Flipped
    {
        get { return flipped; }
    }

    public void Execute(Game game)
    {
        game.Board.Place(Move.Position, placed);
        foreach (Position position in flipped)
            ((Disk)game.Board.GetPiece(position)!).FlipTo(Actor);
    }

    public void Undo(Game game)
    {
        PlayerSide opponent = PlayerSides.Opponent(Actor);
        foreach (Position position in flipped)
            ((Disk)game.Board.GetPiece(position)!).FlipTo(opponent);
        game.Board.Remove(Move.Position);
    }
}
