// Stream 3 self-checks — save/load round-trips across both families, plus
// the gatekeeper requirement that undo works immediately after a load.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Persistence;
using BoardGames.Core.Players;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Persistence;

public class SnapshotMapperTests : IDisposable
{
    private readonly List<string> tempFiles = new List<string>();

    private string TempSavePath()
    {
        string path = Path.Combine(Path.GetTempPath(), "boardgames-test-" + Guid.NewGuid() + ".json");
        tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (string path in tempFiles)
            if (File.Exists(path))
                File.Delete(path);
    }

    private static Move Place(int row, int col)
    {
        return new Move(MoveType.Place, new Position(row, col));
    }

    [Fact]
    public void SaveThenLoad_StandardGomoku_RestoresTheExactBoardAndTurnState()
    {
        IGameFactory factory = new StandardGomokuFactory();
        Game original = factory.CreateGame();
        original.Play(Place(5, 5));
        original.Play(Place(6, 6));
        original.Play(Place(5, 6));

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsHuman, null, null, path);
        LoadedGame loaded = SnapshotMapper.Load(path, factory);

        Assert.Equal('X', loaded.Game.Board.GetPiece(new Position(5, 5))!.Symbol);
        Assert.Equal('O', loaded.Game.Board.GetPiece(new Position(6, 6))!.Symbol);
        Assert.Equal('X', loaded.Game.Board.GetPiece(new Position(5, 6))!.Symbol);
        Assert.Equal(3, loaded.Game.TurnCount);
        Assert.Equal(PlayerSide.PlayerTwo, loaded.Game.ActiveSide);
        Assert.Equal(GameMode.HumanVsHuman, loaded.Mode);
    }

    [Fact]
    public void SaveThenLoad_GomokuPlus_RestoresRemainingInventoryCounts()
    {
        IGameFactory factory = new GomokuPlusFactory();
        Game original = factory.CreateGame();
        original.Play(new Move(MoveType.PlaceHeavy, new Position(4, 4)));   // P1 spends a Heavy
        original.Play(Place(6, 6));
        original.Play(new Move(MoveType.Erase, new Position(6, 6)));        // P1 spends an Eraser

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsComputer, null, AiLevel.Smart, path);
        LoadedGame loaded = SnapshotMapper.Load(path, factory);

        GomokuGame restored = (GomokuGame)loaded.Game;
        Assert.Equal(1, restored.InventoryFor(PlayerSide.PlayerOne).HeavyLeft);
        Assert.Equal(1, restored.InventoryFor(PlayerSide.PlayerOne).EraserLeft);
        Assert.Null(restored.Board.GetPiece(new Position(6, 6)));   // erased stone stays erased
        Assert.Equal(GameMode.HumanVsComputer, loaded.Mode);
        Assert.Null(loaded.PlayerOneAi);
        Assert.Equal(AiLevel.Smart, loaded.PlayerTwoAi);
    }

    [Fact]
    public void SaveThenLoad_Reversi_RestoresFlippedDisksAndDiskCounts()
    {
        IGameFactory factory = new StandardReversiFactory();
        Game original = factory.CreateGame();
        original.Play(Place(3, 4));
        original.Play(Place(3, 3));
        original.Play(Place(4, 3));

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsComputer, null, AiLevel.Dumb, path);
        LoadedGame loaded = SnapshotMapper.Load(path, factory);

        Assert.Equal(original.Board.CountPieces(PlayerSide.PlayerOne),
                    loaded.Game.Board.CountPieces(PlayerSide.PlayerOne));
        Assert.Equal(original.Board.CountPieces(PlayerSide.PlayerTwo),
                    loaded.Game.Board.CountPieces(PlayerSide.PlayerTwo));
        Assert.Equal(original.ActiveSide, loaded.Game.ActiveSide);
    }

    [Fact]
    public void UndoIsImmediatelyAvailable_RightAfterLoading()
    {
        IGameFactory factory = new StandardGomokuFactory();
        Game original = factory.CreateGame();
        original.Play(Place(1, 1));
        original.Play(Place(1, 2));

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsHuman, null, null, path);
        LoadedGame loaded = SnapshotMapper.Load(path, factory);

        Assert.True(loaded.History.CanUndo);
        loaded.History.Undo();

        Assert.Null(loaded.Game.Board.GetPiece(new Position(1, 1)));
        Assert.Null(loaded.Game.Board.GetPiece(new Position(1, 2)));
        Assert.Equal(0, loaded.Game.TurnCount);
    }

    [Fact]
    public void LoadingWithTheWrongVariantFactory_IsRejected()
    {
        IGameFactory standard = new StandardGomokuFactory();
        Game original = standard.CreateGame();
        original.Play(Place(1, 1));
        original.Play(Place(1, 2));

        string path = TempSavePath();
        SnapshotMapper.Save(original, standard, GameMode.HumanVsHuman, null, null, path);

        Assert.Throws<InvalidMoveException>(() => SnapshotMapper.Load(path, new GomokuPlusFactory()));
        Assert.Throws<InvalidMoveException>(() => SnapshotMapper.Load(path, new StandardReversiFactory()));
    }

    [Fact]
    public void SaveThenLoad_GomokuFog_TrueBoardIsFullyRestored_PerspectiveNeedsNoExtraState()
    {
        // FogPerspective is a pure function of the current true board, so a
        // correctly-restored board is a correctly-restored fog view too -
        // nothing variant-specific needs to be saved for GomokuFog at all.
        IGameFactory factory = new GomokuFogFactory();
        Game original = factory.CreateGame();
        original.Play(Place(5, 5));
        original.Play(Place(1, 10));

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsHuman, null, null, path);
        LoadedGame loaded = SnapshotMapper.Load(path, factory);

        Assert.True(loaded.Game.Perspective.IsVisible(loaded.Game, PlayerSide.PlayerOne, new Position(5, 5)));
        Assert.True(loaded.Game.Perspective.IsVisible(loaded.Game, PlayerSide.PlayerOne, new Position(4, 4)));
        Assert.False(loaded.Game.Perspective.IsVisible(loaded.Game, PlayerSide.PlayerOne, new Position(1, 10)));
    }

    [Fact]
    public void SaveFile_IsReadableJson_WithTheExpectedShape()
    {
        IGameFactory factory = new StandardReversiFactory();
        Game original = factory.CreateGame();
        original.Play(Place(3, 4));

        string path = TempSavePath();
        SnapshotMapper.Save(original, factory, GameMode.HumanVsHuman, null, null, path);
        string json = File.ReadAllText(path);

        Assert.Contains("\"GameKey\": \"reversi\"", json);
        Assert.Contains("\"VariantKey\": \"standard\"", json);
        Assert.Contains("\"Type\": \"Place\"", json);
    }
}
