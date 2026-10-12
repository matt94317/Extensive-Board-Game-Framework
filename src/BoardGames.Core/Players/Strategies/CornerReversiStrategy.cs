// Stream 2 — Smarter AI for Corner Reversi: play aggressively for corners
// to reach — or block — a 3-corner sudden-death win. In priority order:
//   1. a corner that is our third: wins on the spot
//   2. never leave the opponent able to take their third corner (blocks)
//   3. take any available corner
//   4. avoid handing the opponent a corner
//   5. otherwise flip as many disks as possible
// Steps 2 and 4 look one move ahead by executing a trial FlipPlaceCommand
// and undoing it again — the same symmetric Execute/Undo the history uses.
using BoardGames.Core.Commands;
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Players.Strategies;

public class CornerReversiStrategy : ReversiScoringStrategy
{
    private const int WinsNow = 1000000;
    private const int LetsOpponentWin = -100000;
    private const int TakesCorner = 1000;
    private const int GivesAwayCorner = -500;

    protected override int Score(Game game, PlayerSide side, Move move, List<Position> flips)
    {
        bool corner = IsCorner(game.Board, move.Position);
        if (corner && CornerDominanceCondition.CornersHeld(game.Board, side)
                      == CornerDominanceCondition.CornersToWin - 1)
            return WinsNow;

        PlayerSide opponent = PlayerSides.Opponent(side);
        FlipPlaceCommand trial = new FlipPlaceCommand(move, side, new Disk(side), flips);
        trial.Execute(game);
        int cornersOpen = CornersAvailableTo(game.Board, opponent);
        bool opponentCanWin = cornersOpen > 0
            && CornerDominanceCondition.CornersHeld(game.Board, opponent)
               == CornerDominanceCondition.CornersToWin - 1;
        trial.Undo(game);

        int score = flips.Count;
        if (opponentCanWin)
            score += LetsOpponentWin;
        else if (cornersOpen > 0)
            score += GivesAwayCorner;
        if (corner)
            score += TakesCorner;
        return score;
    }

    private int CornersAvailableTo(Board board, PlayerSide side)
    {
        int available = 0;
        foreach (Position corner in CornerDominanceCondition.Corners(board))
            if (Flanking.FindFlips(board, corner, side).Count > 0)
                available++;
        return available;
    }
}
