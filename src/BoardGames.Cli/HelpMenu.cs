// Stream 4 - the in-game help menu ('help'). The variant-specific part -
// family, variant title, board size, rules, win condition, PASS rules, piece
// types, remaining GomokuPlus inventory and move syntax - comes from
// game.GetHelp(), which each game family overrides (POLYMORPHISM). This class
// never checks which game it is showing; it only adds the session commands.
using BoardGames.Core.Engine;

namespace BoardGames.Cli
{
    public static class HelpMenu
    {
        private const string Line = "==========================================================";

        public static void ShowGameHelp(Game game)
        {
            Console.WriteLine();
            Console.WriteLine(Line);
            Console.Write(game.GetHelp());
            Console.WriteLine("Rows and columns are numbered from 1, row first: 5:3 = row 5, column 3.");
            Console.WriteLine("Command examples:");
            Console.WriteLine("  help              show this help");
            Console.WriteLine("  undo              take back the last full turn");
            Console.WriteLine("  redo              play an undone turn again (a new move clears redo)");
            Console.WriteLine("  save mygame.json  save the game (no name = " + GameLoop.DefaultSaveFile + ")");
            Console.WriteLine("  load mygame.json  load a saved game");
            Console.WriteLine("  quit              go back to the main menu");
            Console.WriteLine(Line);
        }

        public static void ShowMainMenuHelp()
        {
            Console.WriteLine();
            Console.WriteLine(Line);
            Console.WriteLine("Choose a game by its number, then a mode:");
            Console.WriteLine("  Human vs Human     two players share this terminal");
            Console.WriteLine("  Human vs Computer  you are Player 1 (X) against a Dumb or Smarter AI");
            Console.WriteLine("Type 'help' during a game for that variant's rules and commands.");
            Console.WriteLine("L loads a game saved with 'save'.");
            Console.WriteLine();
            CLIArgumentsParser.PrintUsage();
            Console.WriteLine(Line);
        }
    }
}
