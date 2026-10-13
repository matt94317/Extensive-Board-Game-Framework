// Stream 4 — OBSERVER PATTERN: the terminal view of a game. It is attached
// to the Game (the subject) and redraws the board whenever Update() is
// pushed a GameEvent, so the game loop never has to remember to draw.
//
// The view never decides what a player may see. Every cell is drawn through
// the variant's IPerspective (FullPerspective shows everything,
// FogPerspective masks cells), so the same drawing code serves all six
// variants and hidden cells come out as '?'. Which player's eyes the board
// is drawn through is supplied by the caller (the session or test mode).
using BoardGames.Core.Engine;
using BoardGames.Core.Model;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;

namespace BoardGames.Cli;

public class ConsoleBoardView : IGameObserver
{
    public const char EmptyCell = ' ';
    public const char HiddenCell = '?';

    private readonly Func<PlayerSide> viewer;

    public ConsoleBoardView(Func<PlayerSide> viewer)
    {
        this.viewer = viewer;
    }

    // The side whose perspective the board is currently drawn from.
    protected PlayerSide Viewer
    {
        get { return viewer(); }
    }

    // Picks the view for a game: FogView whenever the variant's perspective
    // can hide cells, the plain view otherwise.
    public static ConsoleBoardView For(Game game, Func<PlayerSide> viewer)
    {
        if (game.Perspective is FullPerspective)
            return new ConsoleBoardView(viewer);
        return new FogView(viewer);
    }

    public void Update(GameEvent gameEvent)
    {
        Console.WriteLine();
        Console.WriteLine(">> " + gameEvent.Description);
        Render(gameEvent.Game);
    }

    // The board as the current viewer sees it, followed by the status line.
    public virtual void Render(Game game)
    {
        Console.WriteLine(game.VariantName + " (" + game.Board.Size + "x" + game.Board.Size + ")");
        DrawGrid(game, Viewer);
        Console.WriteLine(game.StatusLine());
    }

    // The end-of-script rendering used by CLI test mode. Identical to
    // Render() here; FogView adds the true board alongside.
    public virtual void RenderReport(Game game)
    {
        Render(game);
    }

    // Draws the board as a boxed grid with 1-based row and column numbers.
    // A null viewer means the true board: nothing is masked.
    protected void DrawGrid(Game game, PlayerSide? viewer)
    {
        Board board = game.Board;

        string header = "    ";
        for (int col = 1; col <= board.Size; col++)
            header += col.ToString().PadLeft(3).PadRight(4);
        Console.WriteLine(header.TrimEnd());

        string border = "    +";
        for (int col = 1; col <= board.Size; col++)
            border += "---+";
        Console.WriteLine(border);

        for (int row = 1; row <= board.Size; row++)
        {
            string line = row.ToString().PadLeft(3) + " |";
            for (int col = 1; col <= board.Size; col++)
                line += " " + CellSymbol(game, new Position(row, col), viewer) + " |";
            Console.WriteLine(line);
            Console.WriteLine(border);
        }
    }

    private char CellSymbol(Game game, Position position, PlayerSide? viewer)
    {
        if (viewer != null && !game.Perspective.IsVisible(game, viewer.Value, position))
            return HiddenCell;
        Piece? piece = game.Board.GetPiece(position);
        return piece == null ? EmptyCell : piece.Symbol;
    }
}
