using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sudoku
{
    internal class SudokuFileReader
    {
        public static List<int[,]> ReadSudokuFromFile(string filePath)
        {
            List<int[,]> sudokuPuzzles = new List<int[,]>();

            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    int[,] puzzle = PutInArray(line);
                    //Console.WriteLine(line);
                    if (puzzle != null)
                    {
                        sudokuPuzzles.Add(puzzle);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Error reading the file: " + e.Message);
            }

            return sudokuPuzzles;
        }

        private static int[,] PutInArray(string line)
        {
            string[] values = line.Split(' ');

            if (values.Length != 81)
            {
                Console.WriteLine("Invalid Sudoku puzzle format: " + line);
                return null;
            }

            int[,] puzzle = new int[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (int.TryParse(values[i * 9 + j], out int cellValue))
                    {
                        puzzle[i, j] = cellValue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Sudoku cell value: " + values[i * 9 + j]);
                        return null;
                    }
                }
            }

            return puzzle;
        }
    }
}