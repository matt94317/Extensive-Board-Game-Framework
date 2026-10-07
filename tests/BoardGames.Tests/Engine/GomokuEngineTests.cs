// Stream 1 self-checks — engine, Gomoku rules and factories. (Stream 4 owns
// the full suite; these pin down the core contracts the other streams
// build on.)
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Gomoku;
using BoardGames.Core.Variants.Gomoku;

namespace BoardGames.Tests.Engine;

public class GomokuEngineTests
{
    private static Game NewGame(GomokuFactory factory)
    {
        return factory.CreateGame();
    }

    private static Move Place(int row, int col)
    {
        return new Move(MoveType.Place, new Position(row, col));
    }

    [Fact]
    public void PlacingAStone_PutsItOnTheBoard_AndSwapsTheActiveSide()
    {
        Game game = NewGame(new StandardGomokuFactory());
        game.Play(Place(5, 5));

        Assert.NotNull(game.Board.GetPiece(new Position(5, 5)));
        Assert.Equal(PlayerSide.PlayerTwo, game.ActiveSide);
        Assert.Equal(1, game.TurnCount);
    }

    [Fact]
    public void PlacingOnAnOccupiedCell_IsRejected()
    {
        Game game = NewGame(new StandardGomokuFactory());
        game.Play(Place(5, 5));

        Assert.Throws<InvalidMoveException>(() => game.Play(Place(5, 5)));
    }

    [Fact]
    public void FiveInARow_WinsForTheMover_AndFurtherMovesAreRejected()
    {
        Game game = NewGame(new StandardGomokuFactory());
        // P1 builds row 5 cols 1..5; P2 answers in row 9.
        for (int col = 1; col <= 4; col++)
        {
            game.Play(Place(5, col));     // P1
            game.Play(Place(9, col));     // P2
        }
        game.Play(Place(5, 5));           // P1 completes five in a row

        Assert.Equal(GameState.Won, game.Result.State);
        Assert.Equal(PlayerSide.PlayerOne, game.Result.Winner);
        Assert.Throws<InvalidMoveException>(() => game.Play(Place(1, 1)));
    }

    [Fact]
    public void SpecialStones_AreRejectedOutsideGomokuPlus()
    {
        Game game = NewGame(new StandardGomokuFactory());

        Assert.Throws<InvalidMoveException>(
            () => game.Play(new Move(MoveType.PlaceHeavy, new Position(4, 4))));
    }

    [Fact]
    public void Eraser_RemovesAnOrdinaryStone_ButNotAHeavyStone()
    {
        GomokuGame game = (GomokuGame)NewGame(new GomokuPlusFactory());
        game.Play(Place(3, 3));                                        // P1 ordinary
        game.Play(new Move(MoveType.PlaceHeavy, new Position(4, 4)));  // P2 heavy

        // P1 may not erase the Heavy Stone...
        Assert.Throws<InvalidMoveException>(
            () => game.Play(new Move(MoveType.Erase, new Position(4, 4))));

        game.Play(Place(5, 5));                                        // P1 ordinary
        game.Play(new Move(MoveType.Erase, new Position(3, 3)));       // P2 erases P1's stone

        Assert.Null(game.Board.GetPiece(new Position(3, 3)));
        Assert.Equal(1, game.InventoryFor(PlayerSide.PlayerTwo).EraserLeft);
    }

    [Fact]
    public void FogPerspective_ShowsOwnAndAdjacentCells_AndHidesTheRest()
    {
        Game game = NewGame(new GomokuFogFactory());
        game.Play(Place(5, 5));      // P1
        game.Play(Place(1, 10));     // P2 far away

        // P1 sees its stone and a neighbouring cell, but not P2's region.
        Assert.True(game.Perspective.IsVisible(game, PlayerSide.PlayerOne, new Position(5, 5)));
        Assert.True(game.Perspective.IsVisible(game, PlayerSide.PlayerOne, new Position(4, 4)));
        Assert.False(game.Perspective.IsVisible(game, PlayerSide.PlayerOne, new Position(1, 10)));
        Assert.False(game.Perspective.IsVisible(game, PlayerSide.PlayerOne, new Position(1, 9)));
    }

    [Fact]
    public void MoveLog_RecordsCommandsInOrder_ForHistoryAndSaveLoad()
    {
        Game game = NewGame(new StandardGomokuFactory());
        game.Play(Place(1, 1));
        game.Play(Place(2, 2));

        Assert.Equal(2, game.MoveLog.Count);
        Assert.Equal(PlayerSide.PlayerOne, game.MoveLog[0].Actor);
        Assert.Equal(PlayerSide.PlayerTwo, game.MoveLog[1].Actor);
    }

    [Fact]
    public void EveryGomokuFactory_AssemblesItsOwnConsistentKit()
    {
        Assert.IsType<FullPerspective>(NewGame(new StandardGomokuFactory()).Perspective);
        Assert.IsType<FogPerspective>(NewGame(new GomokuFogFactory()).Perspective);

        GomokuGame plus = (GomokuGame)NewGame(new GomokuPlusFactory());
        Assert.True(plus.SpecialStonesEnabled);
        Assert.Equal("plus", new GomokuPlusFactory().VariantKey);
    }
}
