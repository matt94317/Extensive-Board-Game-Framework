// Stream 2 self-checks — Dumb and Smarter AI on hand-built positions, plus
// full AI-vs-AI games in all six variants to prove every move they pick is
// legal and every game reaches an end.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Players;
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Players;

public class StrategyTests
{
    private const PlayerSide X = PlayerSide.PlayerOne;
    private const PlayerSide O = PlayerSide.PlayerTwo;

    private static Game EmptyGame(IGameFactory factory)
    {
        Game game = factory.CreateGame();
        for (int row = 1; row <= game.Board.Size; row++)
            for (int col = 1; col <= game.Board.Size; col++)
                if (!game.Board.IsEmpty(new Position(row, col)))
                    game.Board.Remove(new Position(row, col));
        return game;
    }

    private static void Disks(Game game, PlayerSide side, params int[] rowColPairs)
    {
        for (int i = 0; i < rowColPairs.Length; i += 2)
            game.Board.Place(new Position(rowColPairs[i], rowColPairs[i + 1]), new Disk(side));
    }

    private static void Stones(Game game, PlayerSide side, params int[] rowColPairs)
    {
        for (int i = 0; i < rowColPairs.Length; i += 2)
            game.Board.Place(new Position(rowColPairs[i], rowColPairs[i + 1]), new Stone(side));
    }

    // ---- Dumb AI ----

    [Fact]
    public void RandomStrategy_PicksALegalMove_AndPassesWhenStuck()
    {
        Game game = new StandardReversiFactory().CreateGame();
        Move move = new RandomMoveStrategy(new Random(1)).ChooseMove(game, X);
        Assert.Contains(game.Rules.LegalMoves(game, X), legal => legal.Position.Equals(move.Position));

        Game stuck = EmptyGame(new StandardReversiFactory());
        Disks(stuck, O, 1, 1);
        Disks(stuck, X, 1, 2);
        Assert.Equal(MoveType.Pass, new RandomMoveStrategy(new Random(1)).ChooseMove(stuck, X).Type);
    }

    // ---- Smarter Reversi AIs ----

    // X can flip three in column 4 (5:4) or one in row 6 (6:7).
    private static Game ThreeOrOneFlip(IGameFactory factory)
    {
        Game game = EmptyGame(factory);
        Disks(game, X, 1, 4, 6, 5);
        Disks(game, O, 2, 4, 3, 4, 4, 4, 6, 6);
        return game;
    }

    [Fact]
    public void GreedyStrategy_MaximisesFlips()
    {
        Game game = ThreeOrOneFlip(new StandardReversiFactory());
        Assert.Equal(new Position(5, 4), new GreedyFlipStrategy().ChooseMove(game, X).Position);
    }

    [Fact]
    public void AntiStrategy_MinimisesFlips()
    {
        Game game = ThreeOrOneFlip(new AntiReversiFactory());
        Assert.Equal(new Position(6, 7), new AntiReversiStrategy().ChooseMove(game, X).Position);
    }

    [Fact]
    public void AntiStrategy_AvoidsACorner_EvenWhenItFlipsFewer()
    {
        // 1:1 is a corner flipping one; 5:6 is interior flipping two.
        Game game = EmptyGame(new AntiReversiFactory());
        Disks(game, X, 1, 3, 5, 3);
        Disks(game, O, 1, 2, 5, 4, 5, 5);
        Assert.Equal(new Position(5, 6), new AntiReversiStrategy().ChooseMove(game, X).Position);
    }

    [Fact]
    public void CornerStrategy_TakesTheThirdCorner_OverMoreFlips()
    {
        Game game = EmptyGame(new CornerReversiFactory());
        Disks(game, X, 1, 1, 1, 8, 8, 6, 5, 3);
        Disks(game, O, 8, 7, 5, 4, 5, 5, 5, 6);
        Assert.Equal(new Position(8, 8), new CornerReversiStrategy().ChooseMove(game, X).Position);
    }

    [Fact]
    public void CornerStrategy_BlocksTheOpponentsThirdCorner()
    {
        // O holds two corners and threatens 8:8 through X at 7:7. Playing 5:5
        // flips 6:6 and kills the threat; 4:4 flips more but leaves it open.
        Game game = EmptyGame(new CornerReversiFactory());
        Disks(game, O, 1, 1, 1, 8, 6, 6, 4, 2, 4, 3);
        Disks(game, X, 7, 7, 4, 1);

        Assert.Equal(new Position(5, 5), new CornerReversiStrategy().ChooseMove(game, X).Position);
        Assert.Equal(new Position(4, 4), new GreedyFlipStrategy().ChooseMove(game, X).Position);
        Assert.Equal(O, game.Board.GetPiece(new Position(6, 6))!.Owner);   // lookahead was undone
    }

    // ---- Smarter Gomoku AI ----

    [Fact]
    public void GomokuSmart_CompletesItsOwnFive_BeforeBlocking()
    {
        Game game = EmptyGame(new StandardGomokuFactory());
        Stones(game, X, 5, 1, 5, 2, 5, 3, 5, 4);
        Stones(game, O, 7, 1, 7, 2, 7, 3, 7, 4);
        Assert.Equal(new Position(5, 5), new GomokuSmartStrategy().ChooseMove(game, X).Position);
    }

    [Fact]
    public void GomokuSmart_BlocksAnOpenFour_AndAGappedFour()
    {
        Game open = EmptyGame(new StandardGomokuFactory());
        Stones(open, O, 7, 1, 7, 2, 7, 3, 7, 4);
        Stones(open, X, 2, 2);
        Assert.Equal(new Position(7, 5), new GomokuSmartStrategy().ChooseMove(open, X).Position);

        Game gapped = EmptyGame(new StandardGomokuFactory());
        Stones(gapped, O, 7, 1, 7, 2, 7, 4, 7, 5);
        Stones(gapped, X, 2, 2);
        Assert.Equal(new Position(7, 3), new GomokuSmartStrategy().ChooseMove(gapped, X).Position);
    }

    [Fact]
    public void GomokuSmart_InFog_CannotSeeAHiddenFourToBlockIt()
    {
        Game game = EmptyGame(new GomokuFogFactory());
        Stones(game, O, 7, 1, 7, 2, 7, 3, 7, 4);
        Stones(game, X, 2, 2);
        Assert.NotEqual(new Position(7, 5), new GomokuSmartStrategy().ChooseMove(game, X).Position);
    }

    // ---- players and whole games ----

    [Fact]
    public void ComputerPlayer_DelegatesToItsStrategy()
    {
        Game game = new StandardReversiFactory().CreateGame();
        ComputerPlayer ai = new ComputerPlayer(X, AiLevel.Smart, new GreedyFlipStrategy());

        Assert.True(ai.IsComputer);
        Assert.False(new HumanPlayer(O, "Alice").IsComputer);
        Assert.Equal(AiLevel.Smart, ai.Level);
        Assert.Equal(new GreedyFlipStrategy().ChooseMove(game, X).Position, ai.ChooseMove(game).Position);
    }

    public static IEnumerable<object[]> AllFactories()
    {
        yield return new object[] { new StandardGomokuFactory() };
        yield return new object[] { new GomokuPlusFactory() };
        yield return new object[] { new GomokuFogFactory() };
        yield return new object[] { new StandardReversiFactory() };
        yield return new object[] { new AntiReversiFactory() };
        yield return new object[] { new CornerReversiFactory() };
    }

    [Theory]
    [MemberData(nameof(AllFactories))]
    public void SmartVersusDumb_PlaysAWholeGame_WithOnlyLegalMoves(IGameFactory factory)
    {
        for (int seed = 0; seed < 5; seed++)
        {
            Game game = factory.CreateGame();
            ComputerPlayer smart = new ComputerPlayer(X, AiLevel.Smart, factory.CreateSmartStrategy());
            ComputerPlayer dumb = new ComputerPlayer(O, AiLevel.Dumb, new RandomMoveStrategy(new Random(seed)));

            for (int turn = 0; turn < 200 && game.Result.State == GameState.InProgress; turn++)
            {
                ComputerPlayer mover = game.ActiveSide == X ? smart : dumb;
                game.Play(mover.ChooseMove(game));   // throws if the AI picked an illegal move
            }

            Assert.NotEqual(GameState.InProgress, game.Result.State);
        }
    }
}
