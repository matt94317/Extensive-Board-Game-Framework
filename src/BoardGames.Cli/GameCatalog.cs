// Stream 4 — the six playable variants, each as the ABSTRACT FACTORY that
// builds it plus the move parser for its family. This is the only place in
// the CLI that names concrete factories: the menu, CLI test mode and save
// loading all look variants up here by the same keys the brief's
// --game / --variant arguments use, and from then on work only through
// IGameFactory and IMoveParser. Adding a seventh game is one more entry.
using System.Text.Json;
using BoardGames.Cli.Parsing;
using BoardGames.Core.Engine;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Cli;

public sealed class GameEntry
{
    public IGameFactory Factory { get; }
    public IMoveParser Parser { get; }

    // Display name, e.g. "GomokuPlus", taken from a game the factory builds
    // so the menu never repeats the variant's own naming.
    public string Name { get; }

    public GameEntry(IGameFactory factory, IMoveParser parser)
    {
        Factory = factory;
        Parser = parser;
        Name = factory.CreateGame().VariantName;
    }
}

public static class GameCatalog
{
    private static readonly List<GameEntry> entries = new List<GameEntry>
    {
        new GameEntry(new StandardGomokuFactory(), new GomokuMoveParser()),
        new GameEntry(new GomokuPlusFactory(), new GomokuMoveParser()),
        new GameEntry(new GomokuFogFactory(), new GomokuMoveParser()),
        new GameEntry(new StandardReversiFactory(), new ReversiMoveParser()),
        new GameEntry(new AntiReversiFactory(), new ReversiMoveParser()),
        new GameEntry(new CornerReversiFactory(), new ReversiMoveParser())
    };

    public static IReadOnlyList<GameEntry> All
    {
        get { return entries; }
    }

    public static GameEntry? Find(string gameKey, string variantKey)
    {
        foreach (GameEntry entry in entries)
            if (string.Equals(entry.Factory.GameKey, gameKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Factory.VariantKey, variantKey, StringComparison.OrdinalIgnoreCase))
                return entry;
        return null;
    }

    // Which variant a save file belongs to, so it can be loaded from the
    // main menu without asking the player. Reads only the GameKey and
    // VariantKey that SnapshotMapper.Save() writes; SnapshotMapper.Load()
    // still does the real loading and checks the file again.
    public static GameEntry FindForSaveFile(string path)
    {
        string? gameKey = null;
        string? variantKey = null;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("GameKey", out JsonElement game))
                    gameKey = game.GetString();
                if (root.TryGetProperty("VariantKey", out JsonElement variant))
                    variantKey = variant.GetString();
            }
        }
        catch (JsonException)
        {
            throw new InvalidMoveException("'" + path + "' is not a valid save file.");
        }

        GameEntry? entry = gameKey == null || variantKey == null ? null : Find(gameKey, variantKey);
        if (entry == null)
            throw new InvalidMoveException("'" + path + "' is not a save file for any game this program knows.");
        return entry;
    }
}
