// Stream 1 — which seat a piece or turn belongs to. The engine tracks turns
// by side; the Player classes (Stream 2) decide HOW that side picks a move.
namespace BoardGames.Core.Model;

public enum PlayerSide
{
    None = 0,
    PlayerOne = 1,   // X (dark)
    PlayerTwo = 2    // O (light)
}

public static class PlayerSides
{
    public static PlayerSide Opponent(PlayerSide side)
    {
        if (side == PlayerSide.PlayerOne)
            return PlayerSide.PlayerTwo;
        if (side == PlayerSide.PlayerTwo)
            return PlayerSide.PlayerOne;
        return PlayerSide.None;
    }

    public static string DisplayName(PlayerSide side)
    {
        if (side == PlayerSide.PlayerOne)
            return "Player 1 (X)";
        if (side == PlayerSide.PlayerTwo)
            return "Player 2 (O)";
        return "Nobody";
    }
}
