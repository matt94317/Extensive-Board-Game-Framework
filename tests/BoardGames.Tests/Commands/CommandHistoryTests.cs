// Stream 3 self-checks — turn-level undo/redo, independent of which family
// or variant is being played (CommandHistory itself is variant-agnostic).
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Commands;

public class CommandHistoryTests
{
    private static Move Place(int row, int col)
    {
        return new Move(MoveType.Place, new Position(row, col));
    }

    private static (Game game, CommandHistory history) NewAttached(IGameFactory factory)
    {
        Game game = factory.CreateGame();
        CommandHistory history = new CommandHistory();
        history.Attach(game);
        return (game, history);
    }

    [Fact]
    public void UndoingAFullHvHTurn_RevertsBothMoves_AndReturnsActiveSideToPlayerOne()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(5, 5));    // P1
        game.Play(Place(6, 6));    // P2

        Assert.Equal(1, history.UndoableTurns);

        history.Undo();

        Assert.Null(game.Board.GetPiece(new Position(5, 5)));
        Assert.Null(game.Board.GetPiece(new Position(6, 6)));
        Assert.Equal(0, game.TurnCount);
        Assert.Equal(PlayerSide.PlayerOne, game.ActiveSide);
        Assert.Equal(0, history.UndoableTurns);
        Assert.Equal(1, history.RedoableTurns);
    }

    [Fact]
    public void RedoingATurn_ReappliesBothMoves_ExactlyAsBeforeTheUndo()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(5, 5));
        game.Play(Place(6, 6));
        history.Undo();

        history.Redo();

        Assert.NotNull(game.Board.GetPiece(new Position(5, 5)));
        Assert.NotNull(game.Board.GetPiece(new Position(6, 6)));
        Assert.Equal(2, game.TurnCount);
        Assert.Equal(PlayerSide.PlayerOne, game.ActiveSide);
        Assert.Equal(1, history.UndoableTurns);
        Assert.Equal(0, history.RedoableTurns);
    }

    [Fact]
    public void MultipleTurns_CanBeUndoneOneAfterAnother_InReverseOrder()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(1, 1));    // turn 1: P1
        game.Play(Place(1, 2));    // turn 1: P2
        game.Play(Place(2, 1));    // turn 2: P1
        game.Play(Place(2, 2));    // turn 2: P2

        Assert.Equal(2, history.UndoableTurns);

        history.Undo();            // undoes turn 2
        Assert.Null(game.Board.GetPiece(new Position(2, 1)));
        Assert.Null(game.Board.GetPiece(new Position(2, 2)));
        Assert.NotNull(game.Board.GetPiece(new Position(1, 1)));   // turn 1 still stands

        history.Undo();            // undoes turn 1
        Assert.Null(game.Board.GetPiece(new Position(1, 1)));
        Assert.Null(game.Board.GetPiece(new Position(1, 2)));
        Assert.Equal(0, game.TurnCount);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void PlayingANewMoveAfterUndo_ClearsRedoHistory_BranchingRule()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(1, 1));
        game.Play(Place(1, 2));
        history.Undo();
        Assert.True(history.CanRedo);

        game.Play(Place(9, 9));    // P1 plays a different move instead of redoing
        game.Play(Place(9, 8));

        Assert.False(history.CanRedo);
        Assert.Throws<InvalidMoveException>(() => history.Redo());
    }

    [Fact]
    public void UndoWithNothingToUndo_Throws()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());

        Assert.Throws<InvalidMoveException>(() => history.Undo());
    }

    [Fact]
    public void RedoWithNothingToRedo_Throws()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(1, 1));
        game.Play(Place(1, 2));

        Assert.Throws<InvalidMoveException>(() => history.Redo());
    }

    [Fact]
    public void UndoOrRedo_OnceTheCurrentTurnHasStarted_IsRejected()
    {
        // HvH: P1 has moved, P2 has not yet - the turn is only half-formed,
        // so neither command is available to the player now being asked.
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        game.Play(Place(1, 1));    // P1's half of turn 1

        Assert.True(history.TurnInProgress);
        Assert.Throws<InvalidMoveException>(() => history.Undo());
        Assert.Throws<InvalidMoveException>(() => history.Redo());
    }

    [Fact]
    public void GameEndingMidTurn_SealsASingleCommandTurn_WhichUndoReverts()
    {
        // GomokuPlus HvC-style sequence shortened to a direct win: if the
        // game ends on the very first command played, that lone command
        // must still be its own complete, undoable turn (otherwise the
        // winning move could never be taken back at all).
        (Game game, CommandHistory history) = NewAttached(new StandardGomokuFactory());
        for (int col = 1; col <= 4; col++)
        {
            game.Play(Place(5, col));     // P1
            game.Play(Place(9, col));     // P2
        }
        Assert.Equal(4, history.UndoableTurns);

        game.Play(Place(5, 5));           // P1 completes five in a row and wins
        Assert.Equal(GameState.Won, game.Result.State);
        Assert.Equal(5, history.UndoableTurns);   // the winning half-turn sealed on its own
        Assert.False(history.TurnInProgress);

        history.Undo();

        Assert.Equal(GameState.InProgress, game.Result.State);
        Assert.Null(game.Board.GetPiece(new Position(5, 5)));
        Assert.Equal(PlayerSide.PlayerOne, game.ActiveSide);
    }

    [Fact]
    public void UndoingAReversiTurn_RestoresFlippedDisksOnBothMoves()
    {
        (Game game, CommandHistory history) = NewAttached(new StandardReversiFactory());
        game.Play(new Move(MoveType.Place, new Position(3, 4)));   // P1: flips 4:4
        game.Play(new Move(MoveType.Place, new Position(3, 3)));   // P2: flips 4:4 back

        history.Undo();

        Assert.Null(game.Board.GetPiece(new Position(3, 3)));
        Assert.Null(game.Board.GetPiece(new Position(3, 4)));
        Piece? centre = game.Board.GetPiece(new Position(4, 4));
        Assert.NotNull(centre);
        Assert.Equal(PlayerSide.PlayerTwo, centre!.Owner);   // back to the opening setup
    }

    [Fact]
    public void UndoingAGomokuPlusTurn_RestoresInventoryCounts()
    {
        (Game game, CommandHistory history) = NewAttached(new GomokuPlusFactory());
        GomokuGame gomoku = (GomokuGame)game;
        game.Play(new Move(MoveType.PlaceHeavy, new Position(4, 4)));   // P1 spends a Heavy
        game.Play(Place(6, 6));                                        // P2 ordinary

        Assert.Equal(1, gomoku.InventoryFor(PlayerSide.PlayerOne).HeavyLeft);

        history.Undo();

        Assert.Equal(2, gomoku.InventoryFor(PlayerSide.PlayerOne).HeavyLeft);
        Assert.Null(game.Board.GetPiece(new Position(4, 4)));
    }

    [Fact]
    public void AttachingTwice_IsRejected()
    {
        Game game = new StandardGomokuFactory().CreateGame();
        CommandHistory history = new CommandHistory();
        history.Attach(game);

        Assert.Throws<InvalidOperationException>(() => history.Attach(game));
    }
}
