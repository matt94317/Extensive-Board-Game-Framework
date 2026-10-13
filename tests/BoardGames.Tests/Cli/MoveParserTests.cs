// Stream 4 - the two move parsers turn the brief's command syntax into Moves
// and reject anything that is not valid syntax.
using BoardGames.Cli.Parsing;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Tests.Cli
{
    public class MoveParserTests
    {
        [Theory]
        [InlineData("O5:3", MoveType.Place, 5, 3)]
        [InlineData("H3:3", MoveType.PlaceHeavy, 3, 3)]
        [InlineData("E10:1", MoveType.Erase, 10, 1)]
        [InlineData(" o5:3 ", MoveType.Place, 5, 3)]
        public void GomokuParser_ReadsAllThreeStoneCommands(string text, MoveType type, int row, int col)
        {
            Move move = new GomokuMoveParser().Parse(text);

            Assert.Equal(type, move.Type);
            Assert.Equal(new Position(row, col), move.Position);
        }

        [Theory]
        [InlineData("P3:4", 3, 4)]
        [InlineData("p8:1", 8, 1)]
        public void ReversiParser_ReadsPlaceCommands(string text, int row, int col)
        {
            Move move = new ReversiMoveParser().Parse(text);

            Assert.Equal(MoveType.Place, move.Type);
            Assert.Equal(new Position(row, col), move.Position);
        }

        [Theory]
        [InlineData("PASS")]
        [InlineData("pass")]
        public void ReversiParser_ReadsPass(string text)
        {
            Assert.Equal(MoveType.Pass, new ReversiMoveParser().Parse(text).Type);
        }

        [Theory]
        [InlineData("X5:3")]
        [InlineData("O5")]
        [InlineData("O5:a")]
        [InlineData("O")]
        [InlineData("")]
        public void GomokuParser_RejectsBadSyntax(string text)
        {
            Assert.Throws<InvalidMoveException>(() => new GomokuMoveParser().Parse(text));
        }

        [Theory]
        [InlineData("O3:4")]
        [InlineData("P3")]
        [InlineData("P3:4:5")]
        [InlineData("PAS")]
        public void ReversiParser_RejectsBadSyntax(string text)
        {
            Assert.Throws<InvalidMoveException>(() => new ReversiMoveParser().Parse(text));
        }
    }
}
