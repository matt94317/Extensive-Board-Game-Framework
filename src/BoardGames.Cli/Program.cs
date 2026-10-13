// Stream 4 - the program's entry point. With command-line arguments it runs
// the automated CLI test mode; with none it shows the main menu: pick one of
// the six variants (or load a save), pick Human vs Human or Human vs
// Computer, then play until 'quit' comes back here.
// In a real terminal the menus are driven with the arrow keys (MenuSelector);
// with piped input the same menus take typed answers.
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
                string? choice = ChooseFromMainMenu(variants);
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

        // Shows the main menu and returns the chosen option's key ("1"-"6",
        // "L", "H" or "Q"), or whatever was typed when input is piped.
        private static string? ChooseFromMainMenu(List<IGameFactory> variants)
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("  IFN584 Board Game Framework");
            Console.WriteLine("==========================================");

            List<MenuOption> options = new List<MenuOption>();
            for (int i = 0; i < variants.Count; i++)
            {
                var game = variants[i].CreateGame();
                string key = (i + 1).ToString();
                options.Add(new MenuOption(key, $"{key}. {game.VariantName,-18} {game.Board.Size}x{game.Board.Size}"));
            }
            options.Add(new MenuOption("L", "L. Load a saved game"));
            options.Add(new MenuOption("H", "H. Help"));
            options.Add(new MenuOption("Q", "Q. Quit"));

            if (MenuSelector.IsAvailable)
            {
                int? index = MenuSelector.Select(options, false);
                return options[index ?? 0].Key;
            }

            foreach (MenuOption option in options)
                Console.WriteLine("  " + option.Label);
            return Prompt("> ");
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
        // Returns null if the input is closed, or if the player presses
        // Escape in the arrow-key menu.
        private static int? Choose(string question, params string[] options)
        {
            if (MenuSelector.IsAvailable)
            {
                Console.WriteLine(question);
                List<MenuOption> items = new List<MenuOption>();
                for (int i = 0; i < options.Length; i++)
                    items.Add(new MenuOption((i + 1).ToString(), $"{i + 1}. {options[i]}"));
                int? index = MenuSelector.Select(items, true);
                if (index == null)
                    return null;
                return index + 1;
            }

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
