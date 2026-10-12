using System;

namespace BoardGameFramework.Views
{
    public class FogView
    {
        public void RenderFogBoard(string[,] boardState, bool isPlayerOneView)
        {
            int rows = boardState.GetLength(0);
            int cols = boardState.GetLength(1);

            Console.WriteLine("\n--- GomokuFog Perspective View ---");
            Console.WriteLine("    1  2  3  4  5  6  7  8  9  10");

            for (int i = 0; i < rows; i++)
            {
                Console.Write($"{i + 1,2} ");
                for (int j = 0; j < cols; j++)
                {
                    string cell = boardState[i, j];
                    if (IsHiddenByFog(boardState, i, j, isPlayerOneView))
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

        private bool IsHiddenByFog(string[,] board, int row, int col, bool isP1View)
        {
            string currentCell = board[row, col];
            if (currentCell == "." || (isP1View && currentCell == "X") || (!isP1View && currentCell == "O"))
            {
                return false;
            }

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int r = row + dr;
                    int c = col + dc;
                    if (r >= 0 && r < board.GetLength(0) && c >= 0 && c < board.GetLength(1))
                    {
                        string neighbor = board[r, c];
                        if ((isP1View && neighbor == "X") || (!isP1View && neighbor == "O"))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
