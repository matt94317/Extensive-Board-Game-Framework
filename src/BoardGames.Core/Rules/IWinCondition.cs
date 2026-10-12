// Streams 1 & 2 — STRATEGY PATTERN for outcomes. The engine re-evaluates
// this after every command; swapping the implementation is all it takes to
// turn Standard Reversi into Anti-Reversi (fewest disks) or Corner Reversi
// (3-corner sudden death) without touching the engine.
using BoardGames.Core.Engine;

namespace BoardGames.Core.Rules;

public interface IWinCondition
{
    // Called while the side that just moved is still the active side.
    GameResult Evaluate(Game game);
}
