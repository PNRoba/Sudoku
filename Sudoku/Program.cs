using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sudoku
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "sudoku_puzzles.txt";

            // returns list of sudoku puzzles from file
            List<int[,]> sudokuPuzzles = SudokuFileReader.ReadSudokuFromFile(filePath);

            // Solves and prints:
            // - puzzle
            // - solution
            // - TotalCost + compilation time
            // - Names all violations that are counted by TotalCost
            if (sudokuPuzzles.Count > 0)
            {
                foreach (int[,] puzzle in sudokuPuzzles)
                {
                    GreySudoku greysudoku = new GreySudoku();
                    greysudoku.Grey(puzzle);
                }
            }

            Console.ReadLine();
        }
    }
}
