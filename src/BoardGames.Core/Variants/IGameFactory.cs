// Streams 1 & 2 — ABSTRACT FACTORY contract. A variant is a KIT of products
// that must agree with each other: the configured game, its move rules, its
// win condition, its perspective and its piece factory. Each concrete
// factory assembles one consistent kit; everything outside the factories
// touches variants only through this interface, so adding a seventh game
// means adding classes, never editing the engine (Open-Closed Principle).
using BoardGames.Core.Engine;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Rules;

namespace BoardGames.Core.Variants;

public interface IGameFactory
{
    string GameKey { get; }      // "gomoku" | "reversi"   (CLI --game)
    string VariantKey { get; }   // "standard" | "plus" | "fog" | "anti" | "corner"

    IMoveRules CreateMoveRules();
    IWinCondition CreateWinCondition();
    IPerspective CreatePerspective();
    IPieceFactory CreatePieceFactory();

    // The variant's Smarter AI; the Dumb AI (RandomMoveStrategy) is shared.
    IMoveStrategy CreateSmartStrategy();

    // Assembles the products above into a ready-to-play game.
    Game CreateGame();
}
