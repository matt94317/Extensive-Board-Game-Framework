// Stream 4 — one game being played interactively, from the first move until
// the player quits or loads another save.
//
// Every move, typed by a human or chosen by an AI, goes through the one
// Game.Play() turn algorithm (TEMPLATE METHOD), so the variant's own rules
// validate it. This class only decides WHO supplies the next move and
// handles the session commands: help, undo, redo, save, load and quit.
// The board view is attached as an observer, so it redraws itself after
// every move; the session never draws the board after a move itself.
using System.Text.Json;
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Persistence;
using BoardGames.Core.Players;
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Variants;

namespace BoardGames.Cli;

public sealed class GameSession
{
    public const string DefaultSaveFile = "savegame.json";

    private readonly GameEntry entry;
    private readonly Game game;
    private readonly CommandHistory history;
    private readonly GameMode mode;
    private readonly Player playerOne;
    private readonly Player playerTwo;
    private readonly ConsoleBoardView view;

    private GameSession(GameEntry entry, Game game, CommandHistory history, GameMode mode,
                        Player playerOne, Player playerTwo)
    {
        this.entry = entry;
        this.game = game;
        this.history = history;
        this.mode = mode;
        this.playerOne = playerOne;
        this.playerTwo = playerTwo;
        view = ConsoleBoardView.For(game, ViewerSide);
        game.Attach(view);
    }

    // A fresh game. In Human vs Computer the human is always Player 1 (X):
    // CommandHistory groups each human move with the AI reply after it, so
    // the human's prompt is always at the start of a turn, where undo works.
    public static GameSession Start(GameEntry entry, GameMode mode, AiLevel computerLevel)
    {
        Game game = entry.Factory.CreateGame();
        CommandHistory history = new CommandHistory();
        history.Attach(game);

        Player playerOne = new HumanPlayer(PlayerSide.PlayerOne, PlayerSides.DisplayName(PlayerSide.PlayerOne));
        Player playerTwo = mode == GameMode.HumanVsComputer
            ? CreateComputer(entry.Factory, PlayerSide.PlayerTwo, computerLevel)
            : new HumanPlayer(PlayerSide.PlayerTwo, PlayerSides.DisplayName(PlayerSide.PlayerTwo));
        return new GameSession(entry, game, history, mode, playerOne, playerTwo);
    }

    // Rebuilds a saved game. SnapshotMapper replays its moves into a fresh
    // game with undo history attached; the recorded AI levels say which
    // seats are computers. Prints why and returns null if it cannot load.
    public static GameSession? TryLoad(string path)
    {
        try
        {
            GameEntry entry = GameCatalog.FindForSaveFile(path);
            LoadedGame loaded = SnapshotMapper.Load(path, entry.Factory);
            GameSession session = new GameSession(entry, loaded.Game, loaded.History, loaded.Mode,
                CreatePlayer(entry.Factory, PlayerSide.PlayerOne, loaded.PlayerOneAi),
                CreatePlayer(entry.Factory, PlayerSide.PlayerTwo, loaded.PlayerTwoAi));
            Console.WriteLine("  Loaded " + Path.GetFullPath(path)
                            + " (" + loaded.Game.MoveLog.Count + " moves replayed).");
            return session;
        }
        catch (Exception ex) when (ex is InvalidMoveException || ex is IOException
                                   || ex is UnauthorizedAccessException || ex is JsonException
                                   || ex is ArgumentException)
        {
            Console.WriteLine("  ! Could not load '" + path + "': " + ex.Message);
            return null;
        }
    }

    private static Player CreatePlayer(IGameFactory factory, PlayerSide side, AiLevel? level)
    {
        if (level == null)
            return new HumanPlayer(side, PlayerSides.DisplayName(side));
        return CreateComputer(factory, side, level.Value);
    }

    // Dumb AI is the same random strategy for every variant; the Smarter AI
    // is whichever strategy the variant's factory supplies.
    private static ComputerPlayer CreateComputer(IGameFactory factory, PlayerSide side, AiLevel level)
    {
        IMoveStrategy strategy = level == AiLevel.Dumb
            ? new RandomMoveStrategy(new Random())
            : factory.CreateSmartStrategy();
        return new ComputerPlayer(side, level, strategy);
    }

    // Whose eyes the board is drawn through (it only matters in GomokuFog):
    // in HvC always the human's; in HvH whoever is about to move.
    private PlayerSide ViewerSide()
    {
        if (mode == GameMode.HumanVsComputer)
            return playerOne.IsComputer ? playerTwo.Side : playerOne.Side;
        return game.ActiveSide;
    }

    private Player PlayerFor(PlayerSide side)
    {
        return side == PlayerSide.PlayerOne ? playerOne : playerTwo;
    }

    private string DescribeSession()
    {
        if (mode == GameMode.HumanVsHuman)
            return "Human vs Human";
        Player computer = playerOne.IsComputer ? playerOne : playerTwo;
        Player human = playerOne.IsComputer ? playerTwo : playerOne;
        return "Human vs Computer - you are " + human.Name + ", against " + computer.Name;
    }

    // Plays until the user quits (returns null) or loads a save (returns
    // the session to carry on with).
    public GameSession? Run()
    {
        Console.WriteLine();
        Console.WriteLine("=== " + entry.Name + " - " + DescribeSession() + " ===");
        Console.WriteLine("Type 'help' for the rules and commands.");
        view.Render(game);

        while (true)
        {
            Player current = PlayerFor(game.ActiveSide);
            bool inProgress = game.Result.State == GameState.InProgress;

            if (inProgress && current.IsComputer)
            {
                if (!PlayComputerMove((ComputerPlayer)current))
                    return null;
                continue;
            }

            if (inProgress && game.Rules.LegalMoves(game, current.Side).Count == 0)
                Console.WriteLine("  " + current.Name + " has no legal move and must PASS.");

            Console.Write(inProgress ? current.Name + " > " : "Game over (undo, save, help or quit) > ");
            string? line = Console.ReadLine();
            if (line == null)
                return null;   // input closed
            line = line.Trim();
            if (line.Length == 0)
                continue;

            string[] words = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = words[0].ToLowerInvariant();
            string path = words.Length > 1 ? words[1].Trim() : DefaultSaveFile;

            try
            {
                switch (command)
                {
                    case "help":
                        HelpMenu.ShowGameHelp(game, DescribeSession());
                        break;
                    case "undo":
                        history.Undo();
                        game.Announce("Undo: the last turn was taken back");
                        break;
                    case "redo":
                        history.Redo();
                        game.Announce("Redo: the undone turn was played again");
                        break;
                    case "save":
                        Save(path);
                        break;
                    case "load":
                        GameSession? loaded = TryLoad(path);
                        if (loaded != null)
                        {
                            game.Detach(view);
                            return loaded;
                        }
                        break;
                    case "quit":
                    case "exit":
                        game.Detach(view);
                        return null;
                    default:
                        game.Play(entry.Parser.Parse(line));
                        break;
                }
            }
            catch (InvalidMoveException ex)
            {
                Console.WriteLine("  ! " + ex.Message);
            }
        }
    }

    // Returns false if the AI produced a move its own rules reject, which
    // would otherwise loop forever.
    private bool PlayComputerMove(ComputerPlayer computer)
    {
        Move move = computer.ChooseMove(game);
        Console.WriteLine();
        Console.WriteLine(computer.Name + " plays "
                        + (move.Type == MoveType.Pass ? "PASS" : "at " + move.Position));
        try
        {
            game.Play(move);
            return true;
        }
        catch (InvalidMoveException ex)
        {
            Console.WriteLine("  ! The computer's move was rejected: " + ex.Message);
            return false;
        }
    }

    private void Save(string path)
    {
        try
        {
            SnapshotMapper.Save(game, entry.Factory, mode,
                                AiLevelOf(playerOne), AiLevelOf(playerTwo), path);
            Console.WriteLine("  Saved to " + Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                                   || ex is ArgumentException)
        {
            Console.WriteLine("  ! Could not save to '" + path + "': " + ex.Message);
        }
    }

    private static AiLevel? AiLevelOf(Player player)
    {
        ComputerPlayer? computer = player as ComputerPlayer;
        return computer?.Level;
    }
}
