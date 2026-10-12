// Stream 4 - the program's entry point. With command-line arguments it runs
// the automated CLI test mode; with none it shows the main menu: pick one of
// the six variants (or load a save), pick Human vs Human or Human vs
// Computer, then play until 'quit' comes back here.
using BoardGames.Core.Persistence;
using BoardGames.Core.Players;
using BoardGames.Core.Variants;

namespace BoardGames.Cli
{
    public class Program
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
            List<IGameFactory> variants = GameSelector.AllVariants();

            while (true)
            {
                ShowMainMenu(variants);
                string? choice = Prompt("> ");
                if (choice == null)
                    return;   // input closed

                GameLoop? gameLoop = null;
                switch (choice.ToUpper())
                {
                    case "Q":
                        Console.WriteLine("Goodbye.");
                        return;
                    case "H":
                        HelpMenu.ShowMainMenuHelp();
                        break;
                    case "L":
                        string? path = Prompt("Save file [" + GameLoop.DefaultSaveFile + "]: ");
                        if (path == null)
                            return;
                        if (path.Length == 0)
                            path = GameLoop.DefaultSaveFile;
                        gameLoop = GameLoop.LoadGame(path);
                        break;
                    default:
                        gameLoop = StartNewGame(variants, choice);
                        break;
                }

                // Run() returns the next game when the player loads a save
                // mid-game, and null when they quit back to this menu.
                while (gameLoop != null)
                    gameLoop = gameLoop.Run();
            }
        }

        private static void ShowMainMenu(List<IGameFactory> variants)
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("  IFN584 Board Game Framework");
            Console.WriteLine("==========================================");
            for (int i = 0; i < variants.Count; i++)
            {
                var game = variants[i].CreateGame();
                Console.WriteLine($"  {i + 1}. {game.VariantName,-18} {game.Board.Size}x{game.Board.Size}");
            }
            Console.WriteLine("  L. Load a saved game");
            Console.WriteLine("  H. Help");
            Console.WriteLine("  Q. Quit");
        }

        private static GameLoop? StartNewGame(List<IGameFactory> variants, string choice)
        {
            int number;
            if (!int.TryParse(choice, out number) || number < 1 || number > variants.Count)
            {
                Console.WriteLine($"Please choose 1-{variants.Count}, L, H or Q.");
                return null;
            }
            IGameFactory factory = variants[number - 1];

            int? mode = Choose("Game mode:", "Human vs Human", "Human vs Computer");
            if (mode == null)
                return null;
            if (mode == 1)
                return GameLoop.NewGame(factory, GameMode.HumanVsHuman, AiLevel.Dumb);

            int? level = Choose("Computer difficulty:", "Dumb AI (random legal moves)", "Smarter AI");
            if (level == null)
                return null;
            if (level == 1)
                return GameLoop.NewGame(factory, GameMode.HumanVsComputer, AiLevel.Dumb);
            return GameLoop.NewGame(factory, GameMode.HumanVsComputer, AiLevel.Smart);
        }

        // Asks a numbered question until it gets a valid answer.
        // Returns null if the input is closed.
        private static int? Choose(string question, params string[] options)
        {
            while (true)
            {
                Console.WriteLine(question);
                for (int i = 0; i < options.Length; i++)
                    Console.WriteLine($"  {i + 1}. {options[i]}");

                string? answer = Prompt("> ");
                if (answer == null)
                    return null;
                int number;
                if (int.TryParse(answer, out number) && number >= 1 && number <= options.Length)
                    return number;
                Console.WriteLine($"Please choose 1-{options.Length}.");
            }
        }

        private static string? Prompt(string text)
        {
            Console.Write(text);
            string? line = Console.ReadLine();
            if (line == null)
                return null;
            return line.Trim();
        }
    }
}
