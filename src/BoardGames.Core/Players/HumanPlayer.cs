// Stream 2 — a person at the shared terminal. Their input (moves and
// commands such as undo, save, help) is read and parsed by the CLI, so this
// class only identifies the seat.
using BoardGames.Core.Model;

namespace BoardGames.Core.Players;

public class HumanPlayer : Player
{
    public HumanPlayer(PlayerSide side, string name) : base(side, name) { }

    public override bool IsComputer
    {
        get { return false; }
    }
}
