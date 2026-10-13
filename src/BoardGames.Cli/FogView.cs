// Stream 4 - the GomokuFog view. It inherits all the drawing from
// ConsoleBoardView and overrides Render() so the board is drawn through ONE
// player's eyes: their own stones and the cells next to them are shown, every
// other cell is '?'. The hiding rule itself lives in FogPerspective (Core);
// the true board is never changed, only what is printed.
using BoardGames.Core.Engine;
using BoardGames.Core.Model;

namespace BoardGames.Cli
{
    public class FogView : ConsoleBoardView
    {
        // Human vs Computer: always the human's side.
        // Human vs Human: PlayerSide.None, meaning "whoever is about to move".
        public PlayerSide FixedViewer { get; set; } = PlayerSide.None;

        public override void Render(Game game)
        {
            if (game.Result.State != GameState.InProgress)
            {
                // Once the game is over the fog lifts.
                RenderTrueBoard(game);
            }
            else
            {
                PlayerSide viewer = FixedViewer;
                if (viewer == PlayerSide.None)
                    viewer = game.ActiveSide;
                RenderFogBoard(game, viewer);
            }
            Console.WriteLine(game.StatusLine());
        }

        public void RenderFogBoard(Game game, PlayerSide viewer)
        {
            Console.WriteLine($"\n--- GomokuFog Perspective View: {PlayerSides.DisplayName(viewer)} ('?' = hidden) ---");
            DrawBoard(game, viewer);
        }

        public void RenderTrueBoard(Game game)
        {
            Console.WriteLine("\n--- GomokuFog True Board (what the players cannot see) ---");
            DrawBoard(game, PlayerSide.None);
        }

        // Test mode prints the true board AND the active player's view, so
        // the result of a script can be checked side by side.
        public override void RenderReport(Game game)
        {
            if (game.Result.State == GameState.InProgress)
                RenderTrueBoard(game);
            Render(game);
        }
    }
}
