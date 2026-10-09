// Streams 1 & 2 — what a given viewer is allowed to SEE. The authoritative
// board is never filtered; views (Stream 4) ask this interface per cell and
// mask hidden ones as '?'. Most variants use FullPerspective; GomokuFog
// swaps in FogPerspective via its factory.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Rules;

public interface IPerspective
{
    bool IsVisible(Game game, PlayerSide viewer, Position position);
}
