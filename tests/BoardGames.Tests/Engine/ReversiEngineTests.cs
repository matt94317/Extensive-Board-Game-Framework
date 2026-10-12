// Stream 2 self-checks — the Reversi family end to end through the engine:
// setup, the brief's CLI scripts, PASS, the three win strategies and undo.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Reversi;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Engine;

public class ReversiEngineTests
{
    private static Move P(int row, int col)
    {
        return new Move(MoveType.Place, new Position(row, col));
    }

    private static void PlayScript(Game game, string script)
    {
        foreach (string token in script.Split(','))
        {
            string[] parts = token.Substring(1).Split(':');
            game.Play(P(int.Parse(parts[0]), int.Parse(parts[1])));
        }
    }

    // One board row as symbols, '.' for empty — easy to compare by eye.
    private static string Row(Game game, int row)
    {
        string text = "";
        for (int col = 1; col <= game.Board.Size; col++)
        {
            Piece? piece = game.Board.GetPiece(new Position(row, col));
            text += piece == null ? '.' : piece.Symbol;
        }
        return text;
    }

    // Empties the board so a test can lay out an exact position.
    private static Game EmptyGame(IGameFactory factory)
    {
        Game game = factory.CreateGame();
        for (int row = 1; row <= game.Board.Size; row++)
            for (int col = 1; col <= game.Board.Size; col++)
                if (!game.Board.IsEmpty(new Position(row, col)))
                    game.Board.Remove(new Position(row, col));
        return game;
    }

    private static void Put(Game game, PlayerSide side, params int[] rowColPairs)
    {
        for (int i = 0; i < rowColPairs.Length; i += 2)
            game.Board.Place(new Position(rowColPairs[i], rowColPairs[i + 1]), new Disk(side));
    }

    [Fact]
    public void NewGame_HasTheFourCentreDisks_AndDarkToMoveWithFourOptions()
    {
        Game game = new StandardReversiFactory().CreateGame();

        Assert.Equal("...OX...", Row(game, 4));
        Assert.Equal("...XO...", Row(game, 5));
        Assert.Equal(PlayerSide.PlayerOne, game.ActiveSide);
        Assert.Equal(4, game.Rules.LegalMoves(game, PlayerSide.PlayerOne).Count);
    }

    [Fact]
    public void BriefScript4_StandardReversi_PlaysThroughWithCorrectFlips()
    {
        Game game = new StandardReversiFactory().CreateGame();
        PlayScript(game, "P3:4,P3:3,P4:3,P5:3,P4:2");

        Assert.Equal("..OX....", Row(game, 3));
        Assert.Equal(".XXXX...", Row(game, 4));
        Assert.Equal("..OOO...", Row(game, 5));
        Assert.Equal(5, game.Board.CountPieces(PlayerSide.PlayerOne));
        Assert.Equal(4, game.Board.CountPieces(PlayerSide.PlayerTwo));
        Assert.Equal(GameState.InProgress, game.Result.State);
    }

    [Fact]
    public void BriefScript5_AntiReversi_PlaysThroughWithCorrectFlips()
    {
        Game game = new AntiReversiFactory().CreateGame();
        PlayScript(game, "P4:3,P3:3,P3:4,P5:3,P6:3");

        Assert.Equal("..OX....", Row(game, 3));
        Assert.Equal("..OXX...", Row(game, 4));
        Assert.Equal("..OXO...", Row(game, 5));
        Assert.Equal("..X.....", Row(game, 6));
    }

    [Fact]
    public void BriefScript6_CornerReversi_TakesTheFirstCorner()
    {
        Game game = new CornerReversiFactory().CreateGame();
        PlayScript(game, "P3:4,P3:3,P3:2,P2:2,P1:2,P1:1");

        Assert.Equal("OX......", Row(game, 1));
        Assert.Equal(1, CornerDominanceCondition.CornersHeld(game.Board, PlayerSide.PlayerTwo));
        Assert.Equal(GameState.InProgress, game.Result.State);
    }

    [Fact]
    public void IllegalPlacements_AreRejectedWithoutChangingTheGame()
    {
        Game game = new StandardReversiFactory().CreateGame();

        Assert.Throws<InvalidMoveException>(() => game.Play(P(1, 1)));                    // flips nothing
        Assert.Throws<InvalidMoveException>(() => game.Play(P(4, 4)));                    // occupied
        Assert.Throws<InvalidMoveException>(() => game.Play(P(9, 1)));                    // off the board
        Assert.Throws<InvalidMoveException>(() => game.Play(new Move(MoveType.Erase, new Position(3, 4))));
        Assert.Equal(0, game.TurnCount);
        Assert.Equal(PlayerSide.PlayerOne, game.ActiveSide);
    }

    [Fact]
    public void Pass_IsRejectedWhileALegalMoveExists()
    {
        Game game = new StandardReversiFactory().CreateGame();

        Assert.Throws<InvalidMoveException>(() => game.Play(Move.Pass()));
    }

    [Fact]
    public void Pass_IsAcceptedWhenStuck_AndHandsTheTurnOver()
    {
        // O holds the corner, which X can never flank, so X has no move;
        // O can still play 1:3 to flank X at 1:2.
        Game game = EmptyGame(new StandardReversiFactory());
        Put(game, PlayerSide.PlayerTwo, 1, 1);
        Put(game, PlayerSide.PlayerOne, 1, 2);

        game.Play(Move.Pass());

        Assert.Equal(PlayerSide.PlayerTwo, game.ActiveSide);
        Assert.Equal(GameState.InProgress, game.Result.State);
        Assert.Equal(MoveType.Pass, game.MoveLog[0].Move.Type);
    }

    [Fact]
    public void SameFinalPosition_StandardAwardsMostDisks_AntiAwardsFewest()
    {
        // X plays 1:3, flips the only O disk, and nobody can move again.
        Game standard = EmptyGame(new StandardReversiFactory());
        Put(standard, PlayerSide.PlayerOne, 1, 1);
        Put(standard, PlayerSide.PlayerTwo, 1, 2);
        standard.Play(P(1, 3));

        Game anti = EmptyGame(new AntiReversiFactory());
        Put(anti, PlayerSide.PlayerOne, 1, 1);
        Put(anti, PlayerSide.PlayerTwo, 1, 2);
        anti.Play(P(1, 3));

        Assert.Equal(GameState.Won, standard.Result.State);
        Assert.Equal(PlayerSide.PlayerOne, standard.Result.Winner);
        Assert.Equal(GameState.Won, anti.Result.State);
        Assert.Equal(PlayerSide.PlayerTwo, anti.Result.Winner);
        Assert.Throws<InvalidMoveException>(() => standard.Play(Move.Pass()));   // game over
    }

    [Fact]
    public void EqualDiskCounts_WhenNobodyCanMove_AreADraw()
    {
        // X plays 1:3 to make row 1 XXX; O keeps 8:6-8:8. Neither side can
        // flank anything afterwards, so the game ends 3-3.
        Game game = EmptyGame(new StandardReversiFactory());
        Put(game, PlayerSide.PlayerOne, 1, 1);
        Put(game, PlayerSide.PlayerTwo, 1, 2, 8, 6, 8, 7, 8, 8);

        game.Play(P(1, 3));

        Assert.Equal(GameState.Draw, game.Result.State);
    }

    [Fact]
    public void CornerReversi_ThirdCorner_WinsInstantly_EvenWhileBehindOnDisks()
    {
        Game corner = EmptyGame(new CornerReversiFactory());
        Game standard = EmptyGame(new StandardReversiFactory());
        foreach (Game game in new[] { corner, standard })
        {
            Put(game, PlayerSide.PlayerOne, 1, 1, 1, 8, 6, 6);
            Put(game, PlayerSide.PlayerTwo, 7, 7, 3, 3, 3, 4, 3, 5, 4, 4, 2, 5, 2, 6);
            game.Play(P(8, 8));   // takes the third corner, flipping 7:7
        }

        Assert.True(corner.Board.CountPieces(PlayerSide.PlayerOne)
                  < corner.Board.CountPieces(PlayerSide.PlayerTwo));
        Assert.Equal(GameState.Won, corner.Result.State);
        Assert.Equal(PlayerSide.PlayerOne, corner.Result.Winner);
        // Nobody can move afterwards, so Standard ends on disk count: O wins.
        Assert.Equal(GameState.Won, standard.Result.State);
        Assert.Equal(PlayerSide.PlayerTwo, standard.Result.Winner);
    }

    [Fact]
    public void UndoingAFlipPlace_RestoresEveryFlippedDisk_AndLiftsThePlacedOne()
    {
        Game game = new StandardReversiFactory().CreateGame();
        PlayScript(game, "P3:4,P3:3,P4:3");

        game.MoveLog[2].Undo(game);
        game.MoveLog[1].Undo(game);
        game.MoveLog[0].Undo(game);

        Assert.Equal("........", Row(game, 3));
        Assert.Equal("...OX...", Row(game, 4));
        Assert.Equal("...XO...", Row(game, 5));
    }

    [Fact]
    public void EveryReversiFactory_AssemblesItsOwnConsistentKit()
    {
        IGameFactory[] factories =
        {
            new StandardReversiFactory(), new AntiReversiFactory(), new CornerReversiFactory()
        };
        Type[] winConditions =
        {
            typeof(MostDisksCondition), typeof(FewestDisksCondition), typeof(CornerDominanceCondition)
        };
        string[] keys = { "standard", "anti", "corner" };

        for (int i = 0; i < factories.Length; i++)
        {
            Game game = factories[i].CreateGame();
            Assert.IsType<ReversiGame>(game);
            Assert.Equal("reversi", factories[i].GameKey);
            Assert.Equal(keys[i], factories[i].VariantKey);
            Assert.Equal(8, game.Board.Size);
            Assert.IsType<ReversiMoveRules>(game.Rules);
            Assert.IsType(winConditions[i], game.WinCondition);
            Assert.IsType<FullPerspective>(game.Perspective);
            Assert.Contains(game.VariantName, game.GetHelp());
        }
    }
}
