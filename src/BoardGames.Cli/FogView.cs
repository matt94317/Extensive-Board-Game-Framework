using System;
using BoardGames.Core.Rules;

namespace BoardGames.Cli
{
    public class FogView
    {
        public void RenderFogBoard(string[,] boardState, IPerspective perspective, bool isPlayerOneView)
        {
            int rows = boardState.GetLength(0);
            int cols = boardState.GetLength(1);

            Console.WriteLine("'\n--- GomokuFog Perspective View ---'");
            Console.WriteLine("    1  2  3  4  5  6  7  8  9  10");

            for (int i = 0; i < rows; i++)
            {
                Console.Write($"{i + 1,2} ");
                for (int j = 0; j < cols; j++)
                {
                    string cell = boardState[i, j];
                    if (perspective != null && !perspective.IsVisible(i, j, isPlayerOneView))
                    {
                        Console.Write(" ? ");
                    }
                    else
                    {
                        Console.Write($" {cell} ");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
