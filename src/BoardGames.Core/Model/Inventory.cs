// Stream 1 — one player's stock of GomokuPlus special stones (2 + 2 each).
// Commands spend and refund through this class so undo restores counts exactly.
namespace BoardGames.Core.Model;

public sealed class Inventory
{
    public const int HeavyAllotment = 2;
    public const int EraserAllotment = 2;

    public int HeavyLeft { get; private set; }
    public int EraserLeft { get; private set; }

    public Inventory()
    {
        HeavyLeft = HeavyAllotment;
        EraserLeft = EraserAllotment;
    }

    public void UseHeavy() { HeavyLeft--; }
    public void RestoreHeavy() { HeavyLeft++; }
    public void UseEraser() { EraserLeft--; }
    public void RestoreEraser() { EraserLeft++; }
}
