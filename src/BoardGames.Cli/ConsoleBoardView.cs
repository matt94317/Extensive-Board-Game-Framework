// Stream 4 - OBSERVER PATTERN: the console view of a game. It is attached to
// the Game (the subject) with game.Attach(view); every time a move is played,
// undone or redone the Game calls Update(), and the view redraws the board.
//
// The view never decides what a player may see: each cell is checked with the
// variant's IPerspective, so the same drawing code works for all six variants
// and FogView only has to choose WHOSE perspective to draw.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;

namespace BoardGames.Cli
{
    public class ConsoleBoardView : IGameObserver
    {
        public const char EmptyCell = '.';
        public const char HiddenCell = '?';

        // Picks the right view for a game: FogView when the variant's
        // perspective can hide cells, the normal view otherwise.
        public static ConsoleBoardView CreateFor(Game game)
        {
            if (game.Perspective is FullPerspective)
                return new ConsoleBoardView();
            return new FogView();
        }

        public void Update(GameEvent gameEvent)
        {
            Console.WriteLine();
            Console.WriteLine(">> " + gameEvent.Description);
            Render(gameEvent.Game);
        }

        public virtual void Render(Game game)
        {
            Console.WriteLine($"\n--- {game.VariantName} ({game.Board.Size}x{game.Board.Size}) ---");
            DrawBoard(game, PlayerSide.None);
            Console.WriteLine(game.StatusLine());
        }

        // Used by the CLI test mode at the end of a script.
        public virtual void RenderReport(Game game)
        {
            Render(game);
        }

        // Draws the grid with rows and columns numbered from 1. Cells the
        // viewer cannot see are drawn as '?'. PlayerSide.None = the true board.
        protected void DrawBoard(Game game, PlayerSide viewer)
        {
            int size = game.Board.Size;
            Console.Write("   ");
            for (int col = 1; col <= size; col++)
                Console.Write($"{col,3}");
            Console.WriteLine();

            for (int row = 1; row <= size; row++)
            {
                Console.Write($"{row,3}");
                for (int col = 1; col <= size; col++)
                    Console.Write("  " + CellSymbol(game, new Position(row, col), viewer));
                Console.WriteLine();
            }
        }

        public static char CellSymbol(Game game, Position position, PlayerSide viewer)
        {
            if (viewer != PlayerSide.None && !game.Perspective.IsVisible(game, viewer, position))
                return HiddenCell;

            Piece? piece = game.Board.GetPiece(position);
            if (piece == null)
                return EmptyCell;
            return piece.Symbol;
        }
    }
}
