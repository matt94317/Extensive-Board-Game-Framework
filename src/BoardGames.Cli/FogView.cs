// Stream 4 — the GomokuFog view (Srikesh's FogView, moved into the CLI
// project). The original masked a string[,] copy of the board with its own
// adjacency check; it now asks the variant's IPerspective which cells the
// viewer may see, so the fog rule lives in exactly one place
// (FogPerspective) and the view only presents it. The true board is never
// filtered - only what is drawn.
//
// It specialises ConsoleBoardView in two ways: it names whose perspective is
// on screen, and once the game is over the fog lifts and the true board is
// shown. In test mode it also prints the true board beside the active
// player's view, so a script's result can be checked.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli;

public class FogView : ConsoleBoardView
{
    public FogView(Func<PlayerSide> viewer) : base(viewer) { }

    public override void Render(Game game)
    {
        if (game.Result.State != GameState.InProgress)
        {
            Console.WriteLine("--- " + game.VariantName + ": game over, the fog lifts (true board) ---");
            DrawGrid(game, null);
        }
        else
        {
            Console.WriteLine("--- " + game.VariantName + ": " + PlayerSides.DisplayName(Viewer)
                            + "'s view ('" + HiddenCell + "' = hidden) ---");
            DrawGrid(game, Viewer);
        }
        Console.WriteLine(game.StatusLine());
    }

    public override void RenderReport(Game game)
    {
        if (game.Result.State == GameState.InProgress)
        {
            Console.WriteLine("--- " + game.VariantName + ": true board (hidden from the players) ---");
            DrawGrid(game, null);
            Console.WriteLine();
        }
        Render(game);
    }
}
