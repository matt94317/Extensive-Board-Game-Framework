// Stream 4 - the automated CLI test mode and the help menu. These tests read
// what is written to the console, so they share one collection: xUnit then
// runs them one at a time and they never mix up each other's output.
using BoardGames.Cli;
using BoardGames.Core.Engine;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Tests.Cli
{
    [Collection("Console output")]
    public class CliTestModeTests
    {
        // Runs 'action' and returns everything it wrote to the console.
        private static string CaptureOutput(Action action)
        {
            TextWriter original = Console.Out;
            StringWriter output = new StringWriter();
            Console.SetOut(output);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(original);
            }
            return output.ToString();
        }

        // The six example scripts from the assignment brief (section 4.5).
        [Theory]
        [InlineData("gomoku", "standard", "O5:3,O6:3,O5:4,O6:4,O5:5,O6:5,O5:6,O6:6,O5:7")]
        [InlineData("gomoku", "plus", "O3:2,O3:3,O3:1,O4:3,O3:4,O5:3,E3:3,H3:3")]
        [InlineData("gomoku", "fog", "O5:5,O2:2,O5:6,O2:3,O6:5,O8:8")]
        [InlineData("reversi", "standard", "P3:4,P3:3,P4:3,P5:3,P4:2")]
        [InlineData("reversi", "anti", "P4:3,P3:3,P3:4,P5:3,P6:3")]
        [InlineData("reversi", "corner", "P3:4,P3:3,P3:2,P2:2,P1:2,P1:1")]
        public void BriefScripts_AllRunWithoutARejectedCommand(string game, string variant, string script)
        {
            int exitCode = 0;
            string output = CaptureOutput(() =>
                exitCode = CLIArgumentsParser.Run(new[] { "--game", game, "--variant", variant, script }));

            Assert.Equal(CLIArgumentsParser.Success, exitCode);
            Assert.DoesNotContain("REJECTED", output);
            Assert.Contains("Active player", output);
            Assert.Contains("Piece counts", output);
            Assert.Contains("Result", output);
        }

        [Fact]
        public void StandardGomokuScript_ReportsTheFiveInARowWinner()
        {
            string output = CaptureOutput(() => CLIArgumentsParser.ExecuteTestScript(
                new StandardGomokuFactory(), "O5:3,O6:3,O5:4,O6:4,O5:5,O6:5,O5:6,O6:6,O5:7"));

            Assert.Contains("Result        : Player 1 (X) wins", output);
        }

        [Fact]
        public void FogScript_PrintsTheTrueBoardAndTheMaskedView()
        {
            string output = CaptureOutput(() => CLIArgumentsParser.ExecuteTestScript(
                new GomokuFogFactory(), "O5:5,O2:2"));

            Assert.Contains("GomokuFog True Board", output);
            Assert.Contains("GomokuFog Perspective View", output);
            Assert.Contains("?", output);
        }

        [Fact]
        public void IllegalMove_StopsTheScriptWithAnErrorCode()
        {
            // 1:1 flanks nothing at the start of Reversi, so the rules reject it.
            int exitCode = 0;
            string output = CaptureOutput(() =>
                exitCode = CLIArgumentsParser.ExecuteTestScript(new StandardReversiFactory(), "P1:1,P3:4"));

            Assert.Equal(CLIArgumentsParser.CommandRejected, exitCode);
            Assert.Contains("REJECTED", output);
            Assert.Contains("stopped at command 1", output);
        }

        [Fact]
        public void BadSyntax_IsRejected()
        {
            int exitCode = 0;
            CaptureOutput(() =>
                exitCode = CLIArgumentsParser.ExecuteTestScript(new StandardGomokuFactory(), "O5:5,Z1:1"));

            Assert.Equal(CLIArgumentsParser.CommandRejected, exitCode);
        }

        [Fact]
        public void EraserOnAHeavyStone_IsRejectedInGomokuPlus()
        {
            int exitCode = 0;
            CaptureOutput(() =>
                exitCode = CLIArgumentsParser.ExecuteTestScript(new GomokuPlusFactory(), "H5:5,E5:5"));

            Assert.Equal(CLIArgumentsParser.CommandRejected, exitCode);
        }

        [Theory]
        [InlineData("--game gomoku")]
        [InlineData("--game chess --variant standard A1:1")]
        [InlineData("--game gomoku --variant standard O5:5 extra")]
        public void MissingOrUnknownArguments_ReturnBadArguments(string commandLine)
        {
            string[] args = commandLine.Split(' ');
            int exitCode = 0;
            CaptureOutput(() => exitCode = CLIArgumentsParser.Run(args));

            Assert.Equal(CLIArgumentsParser.BadArguments, exitCode);
        }

        [Fact]
        public void HelpMenu_ShowsTheVariantRulesAndEveryCommand()
        {
            foreach (IGameFactory factory in GameSelector.AllVariants())
            {
                Game game = factory.CreateGame();
                string output = CaptureOutput(() => HelpMenu.ShowGameHelp(game));

                Assert.Contains(game.VariantName, output);
                Assert.Contains(game.Board.Size + "x" + game.Board.Size, output);
                foreach (string command in new[] { "undo", "redo", "save", "load", "quit" })
                    Assert.Contains(command, output);
            }
        }

        [Fact]
        public void GameSelector_FindsAllSixVariantsByTheirCliKeys()
        {
            Assert.Equal(6, GameSelector.AllVariants().Count);
            Assert.IsType<AntiReversiFactory>(GameSelector.FindFactory("reversi", "anti"));
            Assert.IsType<GomokuFogFactory>(GameSelector.FindFactory("GOMOKU", "Fog"));
            Assert.Null(GameSelector.FindFactory("gomoku", "anti"));
        }
    }
}
