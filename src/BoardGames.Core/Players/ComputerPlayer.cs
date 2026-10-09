// Stream 2 — an AI opponent. STRATEGY PATTERN: how it picks a move is
// delegated entirely to the injected IMoveStrategy, so Dumb and Smarter
// play — for any variant — is a matter of which strategy it is given.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Players.Strategies;

namespace BoardGames.Core.Players;

public enum AiLevel
{
    Dumb,
    Smart
}

public class ComputerPlayer : Player
{
    private readonly IMoveStrategy strategy;

    // Recorded so save/load can rebuild the same opponent.
    public AiLevel Level { get; }

    public ComputerPlayer(PlayerSide side, AiLevel level, IMoveStrategy strategy)
        : base(side, level == AiLevel.Dumb ? "Computer (Dumb)" : "Computer (Smarter)")
    {
        Level = level;
        this.strategy = strategy;
    }

    public override bool IsComputer
    {
        get { return true; }
    }

    public Move ChooseMove(Game game)
    {
        return strategy.ChooseMove(game, Side);
    }
}
