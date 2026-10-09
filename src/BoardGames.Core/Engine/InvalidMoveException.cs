// Stream 1 — the framework's single failure mode for rejected input. Rules
// throw it with a human-readable reason; the CLI (Stream 4) catches it and
// shows the message, so illegal input can never corrupt game state.
namespace BoardGames.Core.Engine;

public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}
