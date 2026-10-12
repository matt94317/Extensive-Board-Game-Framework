// Stream 4 - one interactive game, from the first move until the player quits
// or loads another save.
//
// Every move, typed by a human or chosen by the computer, goes through
// Game.Play() (the TEMPLATE METHOD in Core), so the variant's own rules check
// it. This class only decides WHO gives the next move, and handles the
// session commands: help, undo, redo, save, load and quit. The board view is
// attached to the game as an OBSERVER, so it redraws itself after each move.
using System.Text.Json;
using BoardGames.Cli.Parsing;
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Persistence;
using BoardGames.Core.Players;
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Variants;

namespace BoardGames.Cli
{
    public class GameLoop
    {
        public const string DefaultSaveFile = "savegame.json";

        private readonly IGameFactory factory;
        private readonly Game game;
        private readonly CommandHistory history;
        private readonly GameMode mode;
        private readonly Player playerOne;
        private readonly Player playerTwo;
        private readonly IMoveParser parser;
        private readonly ConsoleBoardView view;

        public GameLoop(IGameFactory factory, Game game, CommandHistory history, GameMode mode,
                        Player playerOne, Player playerTwo)
        {
            this.factory = factory;
            this.game = game;
            this.history = history;
            this.mode = mode;
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            parser = GameSelector.CreateParser(factory);

            view = ConsoleBoardView.CreateFor(game);
            if (view is FogView fogView && mode == GameMode.HumanVsComputer)
                fogView.FixedViewer = HumanSide();   // only the human's view is shown
            game.Attach(view);
        }

        // A new game. In Human vs Computer the human is always Player 1 (X).
        public static GameLoop NewGame(IGameFactory factory, GameMode mode, AiLevel computerLevel)
        {
            Game game = factory.CreateGame();
            CommandHistory history = new CommandHistory();
            history.Attach(game);

            Player playerOne = new HumanPlayer(PlayerSide.PlayerOne, PlayerSides.DisplayName(PlayerSide.PlayerOne));
            Player playerTwo;
            if (mode == GameMode.HumanVsComputer)
                playerTwo = CreateComputer(factory, PlayerSide.PlayerTwo, computerLevel);
            else
                playerTwo = new HumanPlayer(PlayerSide.PlayerTwo, PlayerSides.DisplayName(PlayerSide.PlayerTwo));

            return new GameLoop(factory, game, history, mode, playerOne, playerTwo);
        }

        // Loads a save file. The file says which variant it belongs to, so each
        // factory is tried until SnapshotMapper accepts one. Returns null (and
        // prints why) if the file cannot be loaded.
        public static GameLoop? LoadGame(string path)
        {
            try
            {
                foreach (IGameFactory factory in GameSelector.AllVariants())
                {
                    LoadedGame loaded;
                    try
                    {
                        loaded = SnapshotMapper.Load(path, factory);
                    }
                    catch (InvalidMoveException)
                    {
                        continue;   // saved from a different variant
                    }

                    Console.WriteLine($"Loaded {Path.GetFullPath(path)} ({loaded.Game.MoveLog.Count} moves replayed).");
                    return new GameLoop(factory, loaded.Game, loaded.History, loaded.Mode,
                        CreatePlayer(factory, PlayerSide.PlayerOne, loaded.PlayerOneAi),
                        CreatePlayer(factory, PlayerSide.PlayerTwo, loaded.PlayerTwoAi));
                }
                Console.WriteLine("Could not load '" + path + "': it is not a save file for any of the six games.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                Console.WriteLine("Could not load '" + path + "': " + ex.Message);
            }
            return null;
        }

        private static Player CreatePlayer(IGameFactory factory, PlayerSide side, AiLevel? level)
        {
            if (level == null)
                return new HumanPlayer(side, PlayerSides.DisplayName(side));
            return CreateComputer(factory, side, level.Value);
        }

        // STRATEGY PATTERN: the Dumb AI is the same random strategy for every
        // variant; the Smarter AI is whatever strategy the variant's factory gives.
        private static ComputerPlayer CreateComputer(IGameFactory factory, PlayerSide side, AiLevel level)
        {
            IMoveStrategy strategy;
            if (level == AiLevel.Dumb)
                strategy = new RandomMoveStrategy(new Random());
            else
                strategy = factory.CreateSmartStrategy();
            return new ComputerPlayer(side, level, strategy);
        }

        private PlayerSide HumanSide()
        {
            if (playerOne.IsComputer)
                return playerTwo.Side;
            return playerOne.Side;
        }

        private Player CurrentPlayer()
        {
            if (game.ActiveSide == PlayerSide.PlayerOne)
                return playerOne;
            return playerTwo;
        }

        // Plays until the user quits (returns null) or loads a save (returns
        // the loaded game, which the menu then runs).
        public GameLoop? Run()
        {
            Console.WriteLine();
            Console.WriteLine($"=== {game.VariantName} - {DescribeMode()} ===");
            Console.WriteLine("Type 'help' for the rules and commands.");
            view.Render(game);

            while (true)
            {
                Player current = CurrentPlayer();
                bool inProgress = game.Result.State == GameState.InProgress;

                if (inProgress && current is ComputerPlayer computer)
                {
                    if (!PlayComputerMove(computer))
                        return null;
                    continue;
                }

                if (inProgress && game.Rules.LegalMoves(game, current.Side).Count == 0)
                    Console.WriteLine(current.Name + " has no legal move and must PASS.");

                if (inProgress)
                    Console.Write(current.Name + " > ");
                else
                    Console.Write("Game over (undo, save, help or quit) > ");

                string? input = Console.ReadLine();
                if (input == null)
                    return null;   // input closed
                input = input.Trim();
                if (input.Length == 0)
                    continue;

                string[] words = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string command = words[0].ToLower();
                string path = DefaultSaveFile;
                if (words.Length > 1)
                    path = words[1].Trim();

                try
                {
                    switch (command)
                    {
                        case "help":
                            HelpMenu.ShowGameHelp(game);
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
                            GameLoop? loaded = LoadGame(path);
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
                            game.Play(parser.Parse(input));
                            break;
                    }
                }
                catch (InvalidMoveException ex)
                {
                    Console.WriteLine("Invalid: " + ex.Message);
                }
            }
        }

        // Returns false if the computer's move is rejected, so the loop
        // cannot get stuck asking it forever.
        private bool PlayComputerMove(ComputerPlayer computer)
        {
            Move move = computer.ChooseMove(game);
            Console.WriteLine();
            if (move.Type == MoveType.Pass)
                Console.WriteLine(computer.Name + " plays PASS");
            else
                Console.WriteLine(computer.Name + " plays at " + move.Position);

            try
            {
                game.Play(move);
                return true;
            }
            catch (InvalidMoveException ex)
            {
                Console.WriteLine("The computer's move was rejected: " + ex.Message);
                return false;
            }
        }

        private void Save(string path)
        {
            try
            {
                SnapshotMapper.Save(game, factory, mode, AiLevelOf(playerOne), AiLevelOf(playerTwo), path);
                Console.WriteLine("Saved to " + Path.GetFullPath(path));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine("Could not save to '" + path + "': " + ex.Message);
            }
        }

        private static AiLevel? AiLevelOf(Player player)
        {
            if (player is ComputerPlayer computer)
                return computer.Level;
            return null;
        }

        private string DescribeMode()
        {
            if (mode == GameMode.HumanVsHuman)
                return "Human vs Human";
            Player computer = playerOne.IsComputer ? playerOne : playerTwo;
            return "Human vs " + computer.Name;
        }
    }
}
