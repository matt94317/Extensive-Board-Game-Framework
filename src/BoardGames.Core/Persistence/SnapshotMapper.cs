// Stream 3 — Game State Persistence (spec 4.3).
//
// Design choice (see the VIA notes for the full reasoning): rather than
// serialising the live Board/Piece graph, a save file stores only the
// ordered list of moves already validated and played (Game.MoveLog) plus a
// little session metadata (which game/variant, which mode, which AI
// levels). Loading rebuilds the game by asking the SAME IGameFactory the
// game was originally created from for a fresh Game, then replaying every
// move back through Game.Play() - the exact same validated path a human or
// AI move takes during normal play. This is the Abstract Factory and
// Command patterns doing the save/load work for free:
//   - the true board, both inventories, turn count, active side and result
//     all come out correct, because they are rebuilt by the same code that
//     built them the first time - nothing is duplicated or can drift out of
//     sync with Board/Game's own representation;
//   - GomokuFog's "variant-specific perspective state" needs no special
//     handling at all: FogPerspective.IsVisible() is a pure function of the
//     CURRENT true board, so once the true board is correctly restored, the
//     fog view is automatically correct too;
//   - attaching a CommandHistory before replaying means every replayed
//     command is folded into the undo stack exactly as it would have been
//     during live play, which is what satisfies "undo must be immediately
//     available after loading" (spec 4.3) without CommandHistory needing
//     any file-format knowledge of its own.
using System.Text.Json;
using System.Linq;
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Players;
using BoardGames.Core.Variants;

namespace BoardGames.Core.Persistence;

// How the saved session was being played. Stream 4's interactive menu
// chooses this when a game starts; Save() records it, Load() returns it so
// the CLI can rebuild the right Player objects for each side.
public enum GameMode
{
    HumanVsHuman,
    HumanVsComputer
}

// Everything Load() hands back: the replayed game, its ready-to-use
// CommandHistory, and the session metadata needed to reconstruct the two
// Player objects (Stream 2) the way the CLI (Stream 4) originally set them
// up.
public sealed class LoadedGame
{
    public Game Game { get; }
    public CommandHistory History { get; }
    public GameMode Mode { get; }
    public AiLevel? PlayerOneAi { get; }
    public AiLevel? PlayerTwoAi { get; }

    public LoadedGame(Game game, CommandHistory history, GameMode mode,
                      AiLevel? playerOneAi, AiLevel? playerTwoAi)
    {
        Game = game;
        History = history;
        Mode = mode;
        PlayerOneAi = playerOneAi;
        PlayerTwoAi = playerTwoAi;
    }
}

public static class SnapshotMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    // Writes 'game' to 'path'. 'factory' must be the same factory (or an
    // equivalent one for the same game/variant) the game was created from -
    // its GameKey/VariantKey are what Load() later checks the file against.
    public static void Save(Game game, IGameFactory factory, GameMode mode,
                            AiLevel? playerOneAi, AiLevel? playerTwoAi, string path)
    {
        GameSnapshot snapshot = new GameSnapshot
        {
            GameKey = factory.GameKey,
            VariantKey = factory.VariantKey,
            Mode = mode.ToString(),
            PlayerOneAi = playerOneAi?.ToString(),
            PlayerTwoAi = playerTwoAi?.ToString(),
            Moves = game.MoveLog.Select(ToDto).ToList()
        };

        string json = JsonSerializer.Serialize(snapshot, JsonOptions);
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(path, json);
    }

    // Reads 'path' and replays it through 'factory'. Throws
    // InvalidMoveException if the file does not match 'factory's game and
    // variant, or is not a save file this mapper understands; lets the
    // normal file-system exceptions (missing file, no permission, ...)
    // propagate as-is, since those are I/O failures, not rejected input.
    public static LoadedGame Load(string path, IGameFactory factory)
    {
        string json = File.ReadAllText(path);
        GameSnapshot? snapshot = JsonSerializer.Deserialize<GameSnapshot>(json, JsonOptions);
        if (snapshot == null || string.IsNullOrEmpty(snapshot.GameKey))
            throw new InvalidMoveException("'" + path + "' is not a valid save file.");
        if (snapshot.GameKey != factory.GameKey || snapshot.VariantKey != factory.VariantKey)
            throw new InvalidMoveException(
                "'" + path + "' is a save of " + snapshot.GameKey + "/" + snapshot.VariantKey
              + ", not " + factory.GameKey + "/" + factory.VariantKey + ".");

        Game game = factory.CreateGame();
        CommandHistory history = new CommandHistory();
        history.Attach(game);   // before replay, so every replayed move becomes undoable

        foreach (MoveDto moveDto in snapshot.Moves)
            game.Play(ToMove(moveDto));

        GameMode mode = Enum.Parse<GameMode>(snapshot.Mode);
        AiLevel? playerOneAi = ParseAiLevel(snapshot.PlayerOneAi);
        AiLevel? playerTwoAi = ParseAiLevel(snapshot.PlayerTwoAi);
        return new LoadedGame(game, history, mode, playerOneAi, playerTwoAi);
    }

    private static AiLevel? ParseAiLevel(string? value)
    {
        return value == null ? null : Enum.Parse<AiLevel>(value);
    }

    private static MoveDto ToDto(IGameCommand command)
    {
        return new MoveDto
        {
            Type = command.Move.Type.ToString(),
            Row = command.Move.Position.Row,
            Column = command.Move.Position.Column
        };
    }

    private static Move ToMove(MoveDto dto)
    {
        MoveType type = Enum.Parse<MoveType>(dto.Type);
        if (type == MoveType.Pass)
            return Move.Pass();
        return new Move(type, new Position(dto.Row, dto.Column));
    }

    // Plain data shape for the JSON file. Deliberately separate from
    // Move/Position (Stream 1's Model classes, immutable and
    // constructor-only) rather than serialising them directly, so
    // Persistence never needs those classes to change shape for its sake.
    private sealed class GameSnapshot
    {
        public string GameKey { get; set; } = "";
        public string VariantKey { get; set; } = "";
        public string Mode { get; set; } = "";
        public string? PlayerOneAi { get; set; }
        public string? PlayerTwoAi { get; set; }
        public List<MoveDto> Moves { get; set; } = new List<MoveDto>();
    }

    private sealed class MoveDto
    {
        public string Type { get; set; } = "";
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
