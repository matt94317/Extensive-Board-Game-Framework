// Stream 2 — STRATEGY PATTERN contract for AI move selection. Every AI
// level for every variant implements this one method; ComputerPlayer and
// the game loop never know which algorithm they are running.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Players.Strategies;

public interface IMoveStrategy
{
    // Must return a move the game's rules accept for 'side' (Move.Pass()
    // when a Reversi side has no legal placement).
    Move ChooseMove(Game game, PlayerSide side);
}
