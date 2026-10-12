// Stream 2 self-checks — the flanking engine on hand-built 8x8 boards.
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Tests.Rules;

public class FlankingEngineTests
{
    private readonly FlankingEngine engine = new FlankingEngine();

    private static Board OpeningBoard()
    {
        Board board = new Board(8);
        board.Place(new Position(4, 4), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(4, 5), new Disk(PlayerSide.PlayerOne));
        board.Place(new Position(5, 4), new Disk(PlayerSide.PlayerOne));
        board.Place(new Position(5, 5), new Disk(PlayerSide.PlayerTwo));
        return board;
    }

    [Fact]
    public void FromTheOpening_DarkHasExactlyTheFourClassicMoves()
    {
        Board board = OpeningBoard();
        List<Position> legal = new List<Position>();
        for (int row = 1; row <= 8; row++)
            for (int col = 1; col <= 8; col++)
                if (engine.FindFlips(board, new Position(row, col), PlayerSide.PlayerOne).Count > 0)
                    legal.Add(new Position(row, col));

        Assert.Equal(4, legal.Count);
        Assert.Contains(new Position(3, 4), legal);
        Assert.Contains(new Position(4, 3), legal);
        Assert.Contains(new Position(5, 6), legal);
        Assert.Contains(new Position(6, 5), legal);
    }

    [Fact]
    public void OpeningMoveP3_4_FlipsOnlyTheDiskAt4_4()
    {
        List<Position> flips = engine.FindFlips(OpeningBoard(), new Position(3, 4), PlayerSide.PlayerOne);

        Assert.Single(flips);
        Assert.Equal(new Position(4, 4), flips[0]);
    }

    [Fact]
    public void APlacementFlanksInSeveralDirections_AtOnce()
    {
        // X to play at 4:4 closes three lines at once: two O disks to the
        // right, one below and one up-left, each bounded by an X disk.
        Board board = new Board(8);
        board.Place(new Position(4, 5), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(4, 6), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(4, 7), new Disk(PlayerSide.PlayerOne));   // across
        board.Place(new Position(5, 4), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(6, 4), new Disk(PlayerSide.PlayerOne));   // down
        board.Place(new Position(3, 3), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(2, 2), new Disk(PlayerSide.PlayerOne));   // up-left

        List<Position> flips = engine.FindFlips(board, new Position(4, 4), PlayerSide.PlayerOne);

        Assert.Equal(4, flips.Count);
        Assert.Contains(new Position(4, 5), flips);
        Assert.Contains(new Position(4, 6), flips);
        Assert.Contains(new Position(5, 4), flips);
        Assert.Contains(new Position(3, 3), flips);
    }

    [Fact]
    public void ARunThatReachesTheEdgeOrAGap_FlipsNothing()
    {
        Board board = new Board(8);
        board.Place(new Position(1, 2), new Disk(PlayerSide.PlayerTwo));   // runs off the edge
        board.Place(new Position(3, 1), new Disk(PlayerSide.PlayerTwo));
        board.Place(new Position(5, 1), new Disk(PlayerSide.PlayerOne));   // gap at 4:1

        Assert.Empty(engine.FindFlips(board, new Position(1, 1), PlayerSide.PlayerOne));
        Assert.Empty(engine.FindFlips(board, new Position(2, 1), PlayerSide.PlayerOne));
    }

    [Fact]
    public void OccupiedOrOffBoardCells_AreNeverPlacements()
    {
        Board board = OpeningBoard();

        Assert.Empty(engine.FindFlips(board, new Position(4, 4), PlayerSide.PlayerOne));
        Assert.Empty(engine.FindFlips(board, new Position(0, 4), PlayerSide.PlayerOne));
        Assert.Empty(engine.FindFlips(board, new Position(9, 9), PlayerSide.PlayerOne));
    }
}
