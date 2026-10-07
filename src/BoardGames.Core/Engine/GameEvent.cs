// Stream 1 — OBSERVER PATTERN seam. The Game is the subject: after every
// state change it builds a GameEvent and pushes it to every attached
// observer. Views (Stream 4) implement IGameObserver and re-render on
// Update(); the engine itself never writes to the console.
namespace BoardGames.Core.Engine;

public sealed class GameEvent
{
    public Game Game { get; }
    public string Description { get; }

    public GameEvent(Game game, string description)
    {
        Game = game;
        Description = description;
    }
}

public interface IGameObserver
{
    void Update(GameEvent gameEvent);
}
