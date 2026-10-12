// Stream 2 — a seat at the table. The game loop asks each player in turn
// for a move: a computer answers itself through its strategy, a human's
// move is read and parsed by the CLI. HvH is two HumanPlayers, HvC is one
// HumanPlayer and one ComputerPlayer.
using BoardGames.Core.Model;

namespace BoardGames.Core.Players;

public abstract class Player
{
    public PlayerSide Side { get; }
    public string Name { get; }

    protected Player(PlayerSide side, string name)
    {
        Side = side;
        Name = name;
    }

    public abstract bool IsComputer { get; }
}
