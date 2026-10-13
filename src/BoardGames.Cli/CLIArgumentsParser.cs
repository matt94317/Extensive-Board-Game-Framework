// Stream 4 — the automated CLI test mode (Srikesh's CLIArgumentsParser,
// moved into the CLI project). The original only checked each command's
// first letter; it now builds the variant through its factory and plays
// every command through the real Game.Play() turn algorithm, so each one is
// validated by that variant's own rules:
//
//   dotnet run --project src/BoardGames.Cli -- --game reversi --variant anti "P4:3,P3:3"
//
// Afterwards it renders the final board and reports the active player, the
// piece counts and the winner or draw. The first rejected command stops
// the script, with its reason and a non-zero exit code.
using BoardGames.Cli.Parsing;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli;

public static class CLIArgumentsParser
{
    public const int Success = 0;
    public const int CommandRejected = 1;
    public const int BadArguments = 2;

    // Entry point for any command-line arguments; returns the exit code.
    public static int Run(string[] args)
    {
        string? gameKey = null;
        string? variantKey = null;
        string? script = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg == "--help" || arg == "-h")
            {
                PrintUsage();
                return Success;
            }
            if ((arg == "--game" || arg == "--variant") && i + 1 < args.Length)
            {
                if (arg == "--game")
                    gameKey = args[++i];
                else
                    variantKey = args[++i];
            }
            else if (script == null && !arg.StartsWith("--"))
                script = arg;
            else
                return Fail("Unexpected argument '" + arg + "'.");
        }

        if (gameKey == null || variantKey == null || script == null)
            return Fail("Test mode needs --game, --variant and a command script.");

        GameEntry? entry = GameCatalog.Find(gameKey, variantKey);
        if (entry == null)
            return Fail("Unknown game/variant '" + gameKey + " " + variantKey + "'.");

        return ExecuteTestScript(entry, script);
    }

    public static int ExecuteTestScript(GameEntry entry, string scriptSequence)
    {
        string[] commands = scriptSequence.Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (commands.Length == 0)
            return Fail("The command script is empty.");

        Game game = entry.Factory.CreateGame();
        Console.WriteLine("[CLI Test Mode] " + game.VariantName
                        + " (" + game.Board.Size + "x" + game.Board.Size + ")");
        Console.WriteLine("Script: " + scriptSequence);

        for (int step = 1; step <= commands.Length; step++)
        {
            string command = commands[step - 1];
            PlayerSide mover = game.ActiveSide;
            string label = "  " + step.ToString().PadLeft(2) + ". " + command.PadRight(8);
            try
            {
                ValidateAndProcessCommand(game, entry.Parser, command);
            }
            catch (InvalidMoveException ex)
            {
                Console.WriteLine(label + " REJECTED - " + ex.Message);
                Console.WriteLine("Script stopped at command " + step + "; board before it:");
                ReportFinalState(game);
                return CommandRejected;
            }
            Console.WriteLine(label + " ok  (" + PlayerSides.DisplayName(mover) + ")");
        }

        Console.WriteLine();
        ReportFinalState(game);
        return Success;
    }

    // Parses one command in the family's notation and plays it; the parser
    // rejects bad syntax, the variant's rules reject illegal moves.
    private static void ValidateAndProcessCommand(Game game, IMoveParser parser, string command)
    {
        game.Play(parser.Parse(command));
    }

    private static void ReportFinalState(Game game)
    {
        ConsoleBoardView.For(game, () => game.ActiveSide).RenderReport(game);

        bool inProgress = game.Result.State == GameState.InProgress;
        Console.WriteLine("Active player : "
                        + (inProgress ? PlayerSides.DisplayName(game.ActiveSide) : "none (game over)"));
        Console.WriteLine("Pieces        : "
                        + PlayerSides.DisplayName(PlayerSide.PlayerOne) + " "
                        + game.Board.CountPieces(PlayerSide.PlayerOne) + ", "
                        + PlayerSides.DisplayName(PlayerSide.PlayerTwo) + " "
                        + game.Board.CountPieces(PlayerSide.PlayerTwo));
        Console.WriteLine("Result        : " + DescribeResult(game.Result));
    }

    private static string DescribeResult(GameResult result)
    {
        if (result.State == GameState.Won)
            return PlayerSides.DisplayName(result.Winner) + " wins";
        if (result.State == GameState.Draw)
            return "draw";
        return "game in progress";
    }

    private static int Fail(string message)
    {
        Console.WriteLine("[CLI Test Error] " + message);
        PrintUsage();
        return BadArguments;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("CLI test mode:");
        Console.WriteLine("  dotnet run --project src/BoardGames.Cli -- --game <game> --variant <variant> \"<cmd>,<cmd>,...\"");
        Console.WriteLine("  games and variants:");
        foreach (GameEntry entry in GameCatalog.All)
            Console.WriteLine("    --game " + entry.Factory.GameKey.PadRight(8)
                            + " --variant " + entry.Factory.VariantKey.PadRight(9) + " " + entry.Name);
        Console.WriteLine("  commands: Gomoku O[r]:[c], H[r]:[c], E[r]:[c]   Reversi P[r]:[c], PASS");
    }
}
