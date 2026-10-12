// Stream 4 — the in-game help system. Everything variant-specific (family,
// title, board size, rules, win condition, PASS mechanics, piece types,
// remaining GomokuPlus inventory and move syntax) comes POLYMORPHICALLY from
// Game.GetHelp(): each family answers for itself, and this class never asks
// which game it is showing. HelpMenu only frames that text with what the
// CLI alone knows - who is playing, and worked examples of the session
// commands (undo, redo, save, load, quit).
using BoardGames.Core.Engine;

namespace BoardGames.Cli;

public static class HelpMenu
{
    private const string Rule = "============================================================";

    public static void ShowGameHelp(Game game, string session)
    {
        Console.WriteLine();
        Console.WriteLine(Rule);
        Console.Write(game.GetHelp());
        Console.WriteLine("Rows and columns are numbered from 1; row first, e.g. 5:3 = row 5, column 3.");
        Console.WriteLine();
        Console.WriteLine("This session: " + session);
        Console.WriteLine("Command examples:");
        Console.WriteLine("  help              show this help");
        Console.WriteLine("  undo              take back the last full turn (both players' moves)");
        Console.WriteLine("  redo              replay a turn you undid; a new move clears redo history");
        Console.WriteLine("  save mygame.json  save this game (no file name = " + GameSession.DefaultSaveFile + ")");
        Console.WriteLine("  load mygame.json  load a saved game in place of this one");
        Console.WriteLine("  quit              leave this game and return to the main menu");
        Console.WriteLine(Rule);
    }

    public static void ShowMainMenuHelp()
    {
        Console.WriteLine();
        Console.WriteLine(Rule);
        Console.WriteLine("Pick a game by number, then a mode:");
        Console.WriteLine("  Human vs Human     two players take turns at this terminal");
        Console.WriteLine("  Human vs Computer  you are Player 1 (X) against an AI:");
        Console.WriteLine("                     Dumb    - plays a random legal move");
        Console.WriteLine("                     Smarter - Gomoku: wins or blocks five in a row;");
        Console.WriteLine("                               Reversi: plays for flips and corners");
        Console.WriteLine("Type 'help' during a game for that variant's rules and commands.");
        Console.WriteLine("L loads a game saved earlier with 'save'.");
        Console.WriteLine();
        CLIArgumentsParser.PrintUsage();
        Console.WriteLine(Rule);
    }
}
