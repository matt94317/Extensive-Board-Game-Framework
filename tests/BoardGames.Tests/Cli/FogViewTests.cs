// Stream 4 - the views draw each cell through the variant's IPerspective:
// GomokuFog hides cells that are not next to the viewer's own stones, every
// other variant shows the whole board.
using BoardGames.Cli;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Cli
{
    public class FogViewTests
    {
        private static Move Place(int row, int col)
        {
            return new Move(MoveType.Place, new Position(row, col));
        }

        // Player 1 at 5:5, Player 2 at 2:2.
        private static Game FogGameWithTwoStones()
        {
            Game game = new GomokuFogFactory().CreateGame();
            game.Play(Place(5, 5));
            game.Play(Place(2, 2));
            return game;
        }

        [Fact]
        public void FogGame_GetsAFogView_OtherGamesGetThePlainView()
        {
            Assert.IsType<FogView>(ConsoleBoardView.CreateFor(new GomokuFogFactory().CreateGame()));
            Assert.IsType<ConsoleBoardView>(ConsoleBoardView.CreateFor(new StandardGomokuFactory().CreateGame()));
            Assert.IsType<ConsoleBoardView>(ConsoleBoardView.CreateFor(new StandardReversiFactory().CreateGame()));
        }

        [Fact]
        public void Viewer_SeesOwnStoneAndTheEmptyCellsAroundIt()
        {
            Game game = FogGameWithTwoStones();

            Assert.NotEqual(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(5, 5), PlayerSide.PlayerOne));
            Assert.Equal(ConsoleBoardView.EmptyCell,
                ConsoleBoardView.CellSymbol(game, new Position(4, 4), PlayerSide.PlayerOne));
            Assert.Equal(ConsoleBoardView.EmptyCell,
                ConsoleBoardView.CellSymbol(game, new Position(6, 6), PlayerSide.PlayerOne));
        }

        [Fact]
        public void Viewer_CannotSeeOpponentStonesOrFarAwayCells()
        {
            Game game = FogGameWithTwoStones();

            Assert.Equal(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(2, 2), PlayerSide.PlayerOne));
            Assert.Equal(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(10, 10), PlayerSide.PlayerOne));
        }

        [Fact]
        public void EachPlayer_HasTheirOwnPerspective()
        {
            Game game = FogGameWithTwoStones();

            Assert.NotEqual(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(2, 2), PlayerSide.PlayerTwo));
            Assert.Equal(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(5, 5), PlayerSide.PlayerTwo));
        }

        [Fact]
        public void TrueBoard_ShowsEverything_AndIsNeverChangedByTheFog()
        {
            Game game = FogGameWithTwoStones();

            Assert.NotEqual(ConsoleBoardView.HiddenCell,
                ConsoleBoardView.CellSymbol(game, new Position(2, 2), PlayerSide.None));
            Assert.NotNull(game.Board.GetPiece(new Position(2, 2)));
            Assert.NotNull(game.Board.GetPiece(new Position(5, 5)));
        }

        [Fact]
        public void StandardGomoku_NeverHidesACell()
        {
            Game game = new StandardGomokuFactory().CreateGame();
            game.Play(Place(5, 5));

            for (int row = 1; row <= game.Board.Size; row++)
                for (int col = 1; col <= game.Board.Size; col++)
                    Assert.NotEqual(ConsoleBoardView.HiddenCell,
                        ConsoleBoardView.CellSymbol(game, new Position(row, col), PlayerSide.PlayerOne));
        }
    }
}
