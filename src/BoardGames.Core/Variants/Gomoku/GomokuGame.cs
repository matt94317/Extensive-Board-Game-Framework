// Stream 1 — the concrete Game for the whole Gomoku family (10x10). The
// three variants differ only in what their factories inject: Standard and
// Fog play with special stones off (Fog additionally supplies the
// FogPerspective), GomokuPlus turns special stones on and carries one
// Inventory of Heavy/Eraser stones per player.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;

namespace BoardGames.Core.Variants.Gomoku;

public class GomokuGame : Game
{
    public const int BoardSize = 10;

    private readonly Inventory playerOneInventory = new Inventory();
    private readonly Inventory playerTwoInventory = new Inventory();

    public string Variant { get; }
    public bool SpecialStonesEnabled { get; }
    public bool HiddenInformation { get; }
    public IPieceFactory PieceFactory { get; }

    public GomokuGame(string variantName, IMoveRules rules, IWinCondition winCondition,
                      IPerspective perspective, IPieceFactory pieceFactory,
                      bool specialStonesEnabled, bool hiddenInformation)
        : base(new Board(BoardSize), rules, winCondition, perspective)
    {
        Variant = variantName;
        PieceFactory = pieceFactory;
        SpecialStonesEnabled = specialStonesEnabled;
        HiddenInformation = hiddenInformation;
    }

    public override string FamilyName
    {
        get { return "Gomoku"; }
    }

    public override string VariantName
    {
        get { return Variant; }
    }

    public Inventory InventoryFor(PlayerSide side)
    {
        return side == PlayerSide.PlayerOne ? playerOneInventory : playerTwoInventory;
    }

    public override string StatusLine()
    {
        string line = base.StatusLine();
        if (SpecialStonesEnabled && Result.State == GameState.InProgress)
            line += "  [P1 Heavy " + playerOneInventory.HeavyLeft
                  + ", Eraser " + playerOneInventory.EraserLeft
                  + " | P2 Heavy " + playerTwoInventory.HeavyLeft
                  + ", Eraser " + playerTwoInventory.EraserLeft + "]";
        return line;
    }

    public override string GetHelp()
    {
        string help =
            VariantName + " (" + FamilyName + " family, " + BoardSize + "x" + BoardSize + " board)\n"
          + "Goal: be first to align five of your pieces in a row -\n"
          + "horizontally, vertically or diagonally. Full board = draw.\n"
          + "Moves:\n"
          + "  O[row]:[col]   place an ordinary stone (X / O), e.g. O5:3\n";

        if (SpecialStonesEnabled)
            help += "  H[row]:[col]   place a Heavy Stone (@ / #) - cannot be erased\n"
                  + "  E[row]:[col]   erase an opponent ORDINARY stone\n"
                  + "  Remaining - P1: Heavy " + playerOneInventory.HeavyLeft
                  + ", Eraser " + playerOneInventory.EraserLeft
                  + " | P2: Heavy " + playerTwoInventory.HeavyLeft
                  + ", Eraser " + playerTwoInventory.EraserLeft + "\n";

        if (HiddenInformation)
            help += "Fog of war: you see only your own stones and cells adjacent\n"
                  + "to them; everything else shows as '?'. Rules and win checks\n"
                  + "still use the true hidden board.\n";

        help += "Commands: help, undo, redo, save [file], load [file], quit\n";
        return help;
    }
}
