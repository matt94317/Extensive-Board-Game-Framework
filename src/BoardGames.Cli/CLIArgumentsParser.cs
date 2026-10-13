// Stream 4 - the automated CLI test mode (spec 4.5):
//
//   dotnet run --project src/BoardGames.Cli -- --game reversi --variant anti "P4:3,P3:3,P3:4"
//
// The variant is built through its factory, then every command in the
// comma-separated script is parsed and played through Game.Play(), so each
// one is validated by that variant's own rules. At the end the final board is
// rendered and the game status is reported (active player, piece counts,
// winner or draw). No prompts are shown, so it can run unattended.
using BoardGames.Cli.Parsing;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Variants;

namespace BoardGames.Cli
{
    public static class CLIArgumentsParser
    {
        public const int Success = 0;
        public const int CommandRejected = 1;
        public const int BadArguments = 2;

        // Reads --game, --variant and the script from the command line and
        // returns the program's exit code.
        public static int Run(string[] args)
        {
            string? gameKey = null;
            string? variantKey = null;
            string? script = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--game" && i + 1 < args.Length)
                {
                    gameKey = args[i + 1];
                    i++;
                }
                else if (args[i] == "--variant" && i + 1 < args.Length)
                {
                    variantKey = args[i + 1];
                    i++;
                }
                else if (script == null && !args[i].StartsWith("--"))
                {
                    script = args[i];
                }
                else
                {
                    Console.WriteLine("[CLI Test Error] Unexpected argument '" + args[i] + "'.");
                    PrintUsage();
                    return BadArguments;
                }
            }

            if (gameKey == null || variantKey == null || script == null)
            {
                Console.WriteLine("[CLI Test Error] Test mode needs --game, --variant and a command script.");
                PrintUsage();
                return BadArguments;
            }

            IGameFactory? factory = GameSelector.FindFactory(gameKey, variantKey);
            if (factory == null)
            {
                Console.WriteLine("[CLI Test Error] Unknown game/variant: " + gameKey + " " + variantKey);
                PrintUsage();
                return BadArguments;
            }

            return ExecuteTestScript(factory, script);
        }

        public static int ExecuteTestScript(IGameFactory factory, string scriptSequence)
        {
            if (string.IsNullOrWhiteSpace(scriptSequence))
            {
                Console.WriteLine("No command script provided for test mode.");
                return BadArguments;
            }

            Game game = factory.CreateGame();
            IMoveParser parser = GameSelector.CreateParser(factory);
            Console.WriteLine($"[CLI Test Mode] Starting execution for variant: {game.VariantName}");
            Console.WriteLine($"Script: {scriptSequence}");

            string[] commands = scriptSequence.Split(',');
            int step = 0;
            foreach (var rawCommand in commands)
            {
                string command = rawCommand.Trim();
                if (string.IsNullOrEmpty(command)) continue;
                step++;

                PlayerSide mover = game.ActiveSide;
                try
                {
                    game.Play(parser.Parse(command));
                    Console.WriteLine($"[Executing] {step,2}. {command,-8} ok  ({PlayerSides.DisplayName(mover)})");
                }
                catch (InvalidMoveException ex)
                {
                    Console.WriteLine($"[Executing] {step,2}. {command,-8} REJECTED - {ex.Message}");
                    Console.WriteLine($"[CLI Test Error] Script stopped at command {step}. Board before it:");
                    ReportFinalState(game);
                    return CommandRejected;
                }
            }

            Console.WriteLine("[CLI Test Mode] Script execution completed successfully.");
            ReportFinalState(game);
            return Success;
        }

        private static void ReportFinalState(Game game)
        {
            ConsoleBoardView.CreateFor(game).RenderReport(game);

            string activePlayer = "none (game over)";
            if (game.Result.State == GameState.InProgress)
                activePlayer = PlayerSides.DisplayName(game.ActiveSide);

            Console.WriteLine();
            Console.WriteLine("Active player : " + activePlayer);
            Console.WriteLine("Piece counts  : "
                + PlayerSides.DisplayName(PlayerSide.PlayerOne) + " " + game.Board.CountPieces(PlayerSide.PlayerOne) + ", "
                + PlayerSides.DisplayName(PlayerSide.PlayerTwo) + " " + game.Board.CountPieces(PlayerSide.PlayerTwo));
            Console.WriteLine("Result        : " + DescribeResult(game.Result));
        }

        public static string DescribeResult(GameResult result)
        {
            if (result.State == GameState.Won)
                return PlayerSides.DisplayName(result.Winner) + " wins";
            if (result.State == GameState.Draw)
                return "draw";
            return "game in progress";
        }

        public static void PrintUsage()
        {
            Console.WriteLine("CLI test mode:");
            Console.WriteLine("  dotnet run --project src/BoardGames.Cli -- --game <game> --variant <variant> \"<cmd>,<cmd>,...\"");
            Console.WriteLine("  games and variants:");
            foreach (IGameFactory factory in GameSelector.AllVariants())
                Console.WriteLine("    --game " + factory.GameKey + " --variant " + factory.VariantKey);
            Console.WriteLine("  commands: Gomoku O[r]:[c], H[r]:[c], E[r]:[c]   Reversi P[r]:[c], PASS");
        }
    }
}
