// Stream 4 — the program's entry point. With command-line arguments it runs
// the automated CLI test mode; with none it opens the interactive main
// menu: pick one of the six variants (or load a save), pick Human vs Human
// or Human vs Computer and an AI level, then play until 'quit' returns here.
using BoardGames.Core.Persistence;
using BoardGames.Core.Players;

namespace BoardGames.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0)
            return CLIArgumentsParser.Run(args);

        RunMainMenu();
        return 0;
    }

    private static void RunMainMenu()
    {
        while (true)
        {
            ShowMainMenu();
            string? choice = Prompt("> ");
            if (choice == null)
                return;   // input closed

            GameSession? session;
            switch (choice.ToUpperInvariant())
            {
                case "Q":
                case "QUIT":
                    Console.WriteLine("Goodbye.");
                    return;
                case "H":
                case "HELP":
                    HelpMenu.ShowMainMenuHelp();
                    continue;
                case "L":
                case "LOAD":
                    string? path = Prompt("Save file [" + GameSession.DefaultSaveFile + "]: ");
                    if (path == null)
                        return;
                    session = GameSession.TryLoad(path.Length == 0 ? GameSession.DefaultSaveFile : path);
                    break;
                default:
                    session = StartNewGame(choice);
                    break;
            }

            // A session returns the next one when the player loads a save
            // mid-game, and null when they quit back to this menu.
            while (session != null)
                session = session.Run();
        }
    }

    private static void ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("  IFN584 Board Game Framework");
        Console.WriteLine("==========================================");
        for (int i = 0; i < GameCatalog.All.Count; i++)
        {
            GameEntry entry = GameCatalog.All[i];
            int size = entry.Factory.CreateGame().Board.Size;
            Console.WriteLine("  " + (i + 1) + ". " + entry.Name.PadRight(18) + size + "x" + size);
        }
        Console.WriteLine("  L. Load a saved game");
        Console.WriteLine("  H. Help");
        Console.WriteLine("  Q. Quit");
    }

    private static GameSession? StartNewGame(string choice)
    {
        if (!int.TryParse(choice, out int number) || number < 1 || number > GameCatalog.All.Count)
        {
            Console.WriteLine("  ! Choose 1-" + GameCatalog.All.Count + ", L, H or Q.");
            return null;
        }
        GameEntry entry = GameCatalog.All[number - 1];

        int? mode = Choose("Mode for " + entry.Name + ":",
                           "Human vs Human", "Human vs Computer");
        if (mode == null)
            return null;
        if (mode == 1)
            return GameSession.Start(entry, GameMode.HumanVsHuman, AiLevel.Dumb);

        int? level = Choose("Computer difficulty:",
                            "Dumb AI (random legal moves)", "Smarter AI");
        if (level == null)
            return null;
        return GameSession.Start(entry, GameMode.HumanVsComputer,
                                 level == 1 ? AiLevel.Dumb : AiLevel.Smart);
    }

    // Numbered choice; asks again until it gets one. Null if input closes.
    private static int? Choose(string question, params string[] options)
    {
        while (true)
        {
            Console.WriteLine(question);
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine("  " + (i + 1) + ". " + options[i]);
            string? answer = Prompt("> ");
            if (answer == null)
                return null;
            if (int.TryParse(answer, out int number) && number >= 1 && number <= options.Length)
                return number;
            Console.WriteLine("  ! Choose 1-" + options.Length + ".");
        }
    }

    private static string? Prompt(string text)
    {
        Console.Write(text);
        string? line = Console.ReadLine();
        return line?.Trim();
    }
}
