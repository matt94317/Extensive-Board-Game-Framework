// Streams 1 & 2 — the open-information perspective: everything is visible.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Core.Rules;

public class FullPerspective : IPerspective
{
    public bool IsVisible(Game game, PlayerSide viewer, Position position)
    {
        return true;
    }
}
