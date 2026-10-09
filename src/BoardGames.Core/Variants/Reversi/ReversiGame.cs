// Stream 2 — the concrete Game for the whole Reversi family (8x8). Standard,
// Anti and Corner share setup, flanking and PASS; their factories differ
// only in the win condition they inject and the win-rule text shown in help.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;

namespace BoardGames.Core.Variants.Reversi;

public class ReversiGame : Game
{
    public const int BoardSize = 8;

    public string Variant { get; }
    public string WinRule { get; }
    public IPieceFactory PieceFactory { get; }

    public ReversiGame(string variantName, string winRule, IMoveRules rules,
                       IWinCondition winCondition, IPerspective perspective,
                       IPieceFactory pieceFactory)
        : base(new Board(BoardSize), rules, winCondition, perspective)
    {
        Variant = variantName;
        WinRule = winRule;
        PieceFactory = pieceFactory;
        PlaceStartingDisks();
    }

    // The four centre disks are set-up, not moves: they go straight onto the
    // board, so they never appear in the move log and can never be undone.
    private void PlaceStartingDisks()
    {
        PlaceStart(4, 4, PlayerSide.PlayerTwo);
        PlaceStart(4, 5, PlayerSide.PlayerOne);
        PlaceStart(5, 4, PlayerSide.PlayerOne);
        PlaceStart(5, 5, PlayerSide.PlayerTwo);
    }

    private void PlaceStart(int row, int col, PlayerSide owner)
    {
        Position position = new Position(row, col);
        Board.Place(position, PieceFactory.CreatePiece(new Move(MoveType.Place, position), owner));
    }

    public override string FamilyName
    {
        get { return "Reversi"; }
    }

    public override string VariantName
    {
        get { return Variant; }
    }

    public override string StatusLine()
    {
        return base.StatusLine()
             + "  [X: " + Board.CountPieces(PlayerSide.PlayerOne)
             + "  O: " + Board.CountPieces(PlayerSide.PlayerTwo) + "]";
    }

    public override string GetHelp()
    {
        return VariantName + " (" + FamilyName + " family, " + BoardSize + "x" + BoardSize + " board)\n"
             + "Pieces: double-sided disks, X = Dark (Player 1), O = Light (Player 2).\n"
             + "Place a disk on an empty cell so it sandwiches one or more opponent\n"
             + "disks in a straight line (across, down or diagonal) against one of\n"
             + "your own. Every sandwiched disk flips to your colour; a placement\n"
             + "must flip at least one. With no legal move you must PASS.\n"
             + "The game ends when neither player can move or the board is full.\n"
             + "Win: " + WinRule + "\n"
             + "Moves:\n"
             + "  P[row]:[col]   place a disk, e.g. P3:4\n"
             + "  PASS           pass (only when you have no legal move)\n"
             + "Commands: help, undo, redo, save [file], load [file], quit\n";
    }
}
