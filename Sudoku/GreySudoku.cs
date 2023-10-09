using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 1) solution
 2) cost
 3) neighborhood
 4) population
 */

namespace Sudoku
{
    internal class GreySudoku
    {
        static Random random = new Random();
        public void Grey(int[,] sudokuBoard)
        {
            DateTime startTime = DateTime.Now;

            Console.WriteLine("------------------------------------");
            Console.WriteLine("-------------- PUZZLE --------------");
            PrintSudoku(sudokuBoard);
            Console.WriteLine();
            GeneticAlg(sudokuBoard, startTime);

            //Console.ReadLine();
        }

        static (float, List<(int, int[,])>) GenZero(int[,] board, List<(int, int[,])> solutions, float avgFit, int population)
        {
            int TotalFit = 0;
            for (int s = 0; s < population; s++)
            {
                int[,] newboard = CopyBoard(board);
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (newboard[i, j] == 0)
                        {
                            newboard[i, j] = random.Next(1, 10);
                        }
                        else if (board[i, j] == 10)
                        {
                            newboard[i, j] = random.Next(11, 20);
                        }
                    }
                }
                solutions.Add((TotalCost(CopyBoard(newboard)), CopyBoard(newboard)));
                TotalFit += TotalCost(CopyBoard(newboard));
            }
            avgFit = (float) TotalFit / population;
            //Console.WriteLine(avgFit);
            return (avgFit, solutions);
        }

        static int[,] CopyBoard(int[,] board)
        {
            int[,] copy = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    copy[i, j] = board[i, j];
                }
            }
            return copy;
        }

        static void GeneticAlg(int[,] board, DateTime startTime)
        {
            double percent = 0.1;
            int population = 1500;


            (float, List<(int, int[,])>) result = GenZero(board, new List<(int, int[,])>(), -1.0f, population);
            float avgFit = result.Item1;

            List<(int, int[,])> solutions = result.Item2;

            solutions = solutions.OrderBy(item => item.Item1).ToList();
            int rememberCost = solutions[0].Item1;
            DateTime rememberTime = DateTime.Now;
            int totalFit = 0;
            int generations = 1000;
            for (int g = 0; g < generations; g++)
            {
                // choosing parents
                /**
                 * Top 10% solutions will be chosen to remain unchanged
                 * 
                 * The solutions that will repopulate the rest 90% will be chosen at random
                 * but the closer the the rating of the solution is to 0, the higher
                 * the probability it has to be chosen for repopulation
                 */
                //Console.WriteLine("Gen -----------------------------------");

                //for (int i = 0; i < solutions.Count; i++)
                //{
                //    Console.WriteLine(solutions[i]);
                //}

                List<(int, int[,])> nextGen = new List<(int, int[,])>();
                for (int i=0; i < population && nextGen.Count() < population; i++)
                {
                    if (i < (int)(population * percent))
                    {
                        nextGen.Add((TotalCost(CopyBoard(solutions[i].Item2)), CopyBoard(solutions[i].Item2)));
                        //totalFit += TotalCost(solutions[i].Item2);
                    }
                    else
                    {

                        /**
                         *     -------------
                         *     | 1 | 2 | 3 |
                         *     -------------
                         *     | 4 | 5 | 6 |
                         *     -------------
                         *     | 7 | 8 | 9 |
                         *     -------------
                         *     
                         *     for row (chosen3by3 -1 / 3)
                         *     for col ((chosen3by3 -1 % 3)+1)
                         */

                        int rand = random.Next(0, solutions.Count - 1);
                        (int, int[,]) Parent1 = (solutions[rand].Item1, CopyBoard(solutions[rand].Item2));
                        
                        // choosing parents / making children for next generation

                        while (Parent1.Item1 > avgFit)
                        {
                            rand = random.Next(0, solutions.Count - 1);
                            Parent1 = (solutions[rand].Item1, CopyBoard(solutions[rand].Item2));
                        }


                        rand = random.Next(0, solutions.Count - 1);
                        (int, int[,]) Parent2 = (solutions[rand].Item1, CopyBoard(solutions[rand].Item2));
                        if (random.NextDouble()<=0.4)
                        {
                            while (Parent2.Item1 > avgFit || Parent1.Item1 == Parent2.Item1)
                            {
                                rand = random.Next(0, solutions.Count - 1);
                                Parent2 = (solutions[rand].Item1, CopyBoard(solutions[rand].Item2));
                            }
                        }
                        else
                        {
                            while (Parent2.Item1 < avgFit || Parent1.Item1 == Parent2.Item1)
                            {
                                rand = random.Next(0, solutions.Count - 1);
                                Parent2 = (solutions[rand].Item1, CopyBoard(solutions[rand].Item2));
                            }
                        }
                        

                        ((int, int[,]), (int, int[,])) children = UniformCrossover(Parent1, Parent2);
                        
                        nextGen.Add((TotalCost(CopyBoard(children.Item1.Item2)), CopyBoard(children.Item1.Item2)));
                        if (nextGen.Count >= population)
                        {
                            i = population;
                            
                        }
                        else nextGen.Add((TotalCost(CopyBoard(children.Item2.Item2)), CopyBoard(children.Item2.Item2)));
                        //totalFit += TotalCost(child.Item2);
                    }
                }

                // mutating solutions

                for (int i = (int)(nextGen.Count * percent); i < nextGen.Count; i++) 
                {
                    int[,] copy = CopyBoard(nextGen[i].Item2);
                    //mutation by switching places of 2 numbers in a row or column

                    if (random.NextDouble() <= 0.4)
                    {
                        int num = random.Next(0, 9);

                        if (random.NextDouble() <= 0.5) // if row or column
                        {
                            // if row
                            // check if row has 2 places that allow swapping
                            HashSet<int> set = new HashSet<int>();
                            for (int j = 0; j < 9; j++)
                            {
                                if (board[num, j] % 10 == 0)
                                {
                                    set.Add(j);
                                }
                            }
                            if (set.Count < 2) break;
                            int col1 = random.Next(0, 9);
                            while (!set.Contains(col1))
                            {
                                col1 = random.Next(0, 9);
                            }
                            int col2 = random.Next(0, 9);
                            while (!set.Contains(col2))
                            {
                                col2 = random.Next(0, 9);
                            }
                            if (copy[num, col1] == copy[num, col2])
                            {
                                int[,] c1 = CopyBoard(copy);
                                int[,] c2 = CopyBoard(copy);

                                c1[num, col1] = random.Next(1, 10) + ((c1[num, col1] / 10) * 10);
                                c2[num, col1] = random.Next(1, 10) + ((c2[num, col1] / 10) * 10);


                                if (TotalCost(c1) > TotalCost(c2))
                                {
                                    copy = CopyBoard(c2);
                                }
                                else
                                {
                                    copy = CopyBoard(c1);
                                }
                            }
                            else
                            {
                                int temp = copy[num, col1] % 10;
                                copy[num, col1] = copy[num, col2] % 10 + ((copy[num, col1] / 10) * 10);
                                copy[num, col2] = temp + ((copy[num, col2] / 10) * 10);
                            }
                        }
                        else
                        {
                            // if column
                            // check if column has 2 places that allow swapping
                            HashSet<int> set = new HashSet<int>();
                            for (int j = 0; j < 9; j++)
                            {
                                if (board[j, num] % 10 == 0)
                                {
                                    set.Add(j);
                                }
                            }
                            if (set.Count < 2) break;
                            int row1 = random.Next(0, 9);
                            while (!set.Contains(row1))
                            {
                                row1 = random.Next(0, 9);
                            }
                            int row2 = random.Next(0, 9);
                            while (!set.Contains(row2))
                            {
                                row2 = random.Next(0, 9);
                            }
                            if (copy[row1, num] == copy[row2, num])
                            {
                                int[,] c1 = CopyBoard(copy);
                                int[,] c2 = CopyBoard(copy);

                                c1[row1, num] = random.Next(1, 10) + ((c1[row1, num] / 10) * 10);
                                c2[row2, num] = random.Next(1, 10) + ((c2[row2, num] / 10) * 10);

                                if (TotalCost(c1) > TotalCost(c2))
                                {
                                    copy = CopyBoard(c2);

                                }
                                else
                                {
                                    copy = CopyBoard(c1);
                                }

                            }
                            else
                            {
                                int temp = copy[row1, num] % 10;
                                copy[row1, num] = copy[row2, num] % 10 + ((copy[row1, num] / 10) * 10);
                                copy[row2, num] = temp + ((copy[row2, num] / 10) * 10);

                            }

                        }
                    }
                    else if (random.NextDouble() > 0.4 && random.NextDouble() <= 0.8)
                    {
                        // mutation to random
                            for (int j = 0; j < random.Next(1, 4); j++)
                            {
                                int row = random.Next(0, 9);
                                int col = random.Next(0, 9);

                                while (board[row, col] % 10 != 0)
                                {
                                    row = random.Next(0, 9);
                                    col = random.Next(0, 9);
                                }

                                int num = random.Next(1, 10);
                                while (board[row, col] % 10 == num) // make sure there is a mutation
                                {
                                    num = random.Next(1, 10);
                                }

                                if (copy[row, col] >= 10)
                                {
                                    copy[row, col] = num + 10;
                                }
                                else
                                {
                                    copy[row, col] = num;
                                }
                                nextGen[i] = ((TotalCost(CopyBoard(copy)), CopyBoard(copy)));
                            }
                    }

                    nextGen[i] = ((TotalCost(CopyBoard(copy)), CopyBoard(copy)));
                }

                solutions = CopyList(nextGen);
                solutions = solutions.OrderBy(item => item.Item1).ToList();

                //if (g % 10 == 0) // g % 50 == 0
                //{
                //    //for (int i = 0; i < solutions.Count; i++)
                //    //{
                //    //    Console.WriteLine(solutions[i]);
                //    //}
                //    DateTime endTime = DateTime.Now;
                //    TimeSpan compilationTime = endTime - startTime;
                //    Console.WriteLine("-------------------------------------------------");
                //    Console.WriteLine("gen:" + g + " / " + generations + " || best solution is: best = " + nextGen[0].Item1 + "|| worst = " + nextGen[nextGen.Count - 1].Item1 + $"  || Compilation Time: {compilationTime.TotalSeconds} seconds");
                //}


                totalFit = 0;
                foreach (var tuple in solutions)
                {
                    int intValue = tuple.Item1;

                    totalFit += intValue;
                }
                avgFit = totalFit / population;

                // remembers the last time cost decreased and what was the compilation time
                if (rememberCost > solutions[0].Item1)
                {
                    rememberCost = solutions[0].Item1;
                    rememberTime = DateTime.Now;
                }
                if (solutions[0].Item1 == 0) break;
            }
            Console.WriteLine("------------- SOLUTION -------------");
            PrintSudoku(solutions[0].Item2);
            TimeSpan compilationTime = rememberTime - startTime;
            Console.WriteLine("Best value [ "+ TotalCost(solutions[0].Item2) + " ] reached in "+ compilationTime.TotalSeconds + " seconds");
            WhatIsViolated(solutions[0].Item2);
        }

        static ((int, int[,]), (int, int[,])) UniformCrossover((int, int[,]) parent1, (int, int[,]) parent2)
        {
            int size = parent1.Item2.GetLength(0);
            int[,] child1 = new int[size, size];
            int[,] child2 = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (random.NextDouble() < 0.5) // 50% chance to take gene from Parent 1
                    {
                        child1[i, j] = parent1.Item2[i, j];
                        child2[i, j] = parent2.Item2[i, j];
                    }
                    else // 50% chance to take gene from Parent 2
                    {
                        child1[i, j] = parent2.Item2[i, j];
                        child2[i, j] = parent1.Item2[i, j];
                    }
                }
            }         
            return ((TotalCost(child1), child1), (TotalCost(child2), child2));
        }

        static List<(int, int[,])> CopyList(List<(int, int[,])> originalList)
        {
            List<(int, int[,])> copiedList = new List<(int, int[,])>();

            foreach (var tuple in originalList)
            {
                int intValue = tuple.Item1;
                int[,] originalArray = tuple.Item2;

                int[,] copiedArray = new int[originalArray.GetLength(0), originalArray.GetLength(1)];
                for (int i = 0; i < originalArray.GetLength(0); i++)
                {
                    for (int j = 0; j < originalArray.GetLength(1); j++)
                    {
                        copiedArray[i, j] = originalArray[i, j];
                    }
                }

                copiedList.Add((intValue, copiedArray));
            }

            return copiedList;
        }


        static void WhatIsViolated(int[,] board)
        {
            int v = 1;
            Console.WriteLine("Violations:");
            // Check rows
            for (int i = 0; i < 9; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                for (int j = 0; j < 9; j++)
                {
                    int number = board[i, j] % 10;
                    if (number != 0 && !rowSet.Add(number))
                    {
                        Console.WriteLine(v+") Violation, twice in row["+ i + "]: " + number);
                        v++;
                    }
                }
            }

            // Check columns
            for (int j = 0; j < 9; j++)
            {
                HashSet<int> colSet = new HashSet<int>();
                for (int i = 0; i < 9; i++)
                {
                    int number = board[i, j] % 10;
                    if (number != 0 && !colSet.Add(number))
                    {
                        Console.WriteLine(v+") Violation, twice in column[" + j + "]: " + number);
                        v++;
                    }
                }
            }

            // Check 3x3 subgrids
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    HashSet<int> subgridSet = new HashSet<int>();
                    for (int i = blockRow * 3; i < (blockRow + 1) * 3; i++)
                    {
                        for (int j = blockCol * 3; j < (blockCol + 1) * 3; j++)
                        {
                            int number = board[i, j] % 10;
                            if (number != 0 && !subgridSet.Add(number))
                            {
                                Console.WriteLine(v+") Violation, twice in block row[" + (int)(blockRow+1) + "]col[" + (int)(blockCol +1) + "]: " + number);
                                v++;
                            }
                        }
                    }
                }
            }

            // Check grey + if there are zeros still in the board
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (board[i, j] / 10 == 1) // if is grey
                    {
                        if (j + 1 < 9 && i + 1 < 9 && (!(board[i + 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i - 1 > -1 && !(board[i - 1, j] % 10 == board[i, j - 1] % 10))
                        && (j + 1 < 9 && i - 1 > -1 && !(board[i - 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i + 1 < 9 && !(board[i + 1, j] % 10 == board[i, j - 1] % 10)))
                        {
                            Console.WriteLine(v + ") Violation, grey row[" + i + "]col[" + j + "] has only unique orthogonal neighbors");
                            v++;
                        }
                    }
                    else // if is white
                    {
                        if (j + 1 < 9 && i + 1 < 9 && ((board[i + 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i - 1 > -1 && (board[i - 1, j] % 10 == board[i, j - 1] % 10))
                        && (j + 1 < 9 && i - 1 > -1 && (board[i - 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i + 1 < 9 && (board[i + 1, j] % 10 == board[i, j - 1] % 10))) 
                        {
                            Console.WriteLine(v + ") Violation, white row[" + i + "]col[" + j + "] has similair orthogonal neighbors");
                            v++;
                        }
                    }
                }
            }
        }

        static int TotalCost(int[,] board)
        {
            int rating = 0;



            // Check rows
            for (int i = 0; i < 9; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                for (int j = 0; j < 9; j++)
                {
                    int number = board[i, j] % 10;
                    if (number != 0 && !rowSet.Add(number))
                    {
                        rating++;
                    }
                }
            }

            // Check columns
            for (int j = 0; j < 9; j++)
            {
                HashSet<int> colSet = new HashSet<int>();
                for (int i = 0; i < 9; i++)
                {
                    int number = board[i, j] % 10;
                    if (number != 0 && !colSet.Add(number))
                    {
                        rating++;
                    }
                }
            }

            // Check 3x3 subgrids
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    HashSet<int> subgridSet = new HashSet<int>();
                    for (int i = blockRow * 3; i < (blockRow + 1) * 3; i++)
                    {
                        for (int j = blockCol * 3; j < (blockCol + 1) * 3; j++)
                        {
                            int number = board[i, j] % 10;
                            if (number != 0 && !subgridSet.Add(number))
                            {
                                rating++;
                            }
                        }
                    }
                }
            }

            // Check grey + if there are zeros still in the board
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (board[i, j] / 10 == 1) // if is grey
                    {
                        if (j + 1 < 9 && i + 1 < 9 && (!(board[i + 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i - 1 > -1 && !(board[i - 1, j] % 10 == board[i, j - 1] % 10))
                        && (j + 1 < 9 && i - 1 > -1 && !(board[i - 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i + 1 < 9 && !(board[i + 1, j] % 10 == board[i, j - 1] % 10)))
                            rating++;
                    }
                    else // if is white
                    {
                        if (j + 1 < 9 && i + 1 < 9 && ((board[i + 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i - 1 > -1 && (board[i - 1, j] % 10 == board[i, j - 1] % 10))
                        && (j + 1 < 9 && i - 1 > -1 && (board[i - 1, j] % 10 == board[i, j + 1] % 10))
                        && (j - 1 > -1 && i + 1 < 9 && (board[i + 1, j] % 10 == board[i, j - 1] % 10)))
                            rating ++;
                    }
                }
            }

            return rating;
        }

        static void PrintSudoku(int[,] board)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("      | - 1 - | - 2 - | - 3 - |");
            Console.WriteLine("      | 0 1 2 | 3 4 5 | 6 7 8 |");
            for (int i = 0; i < 9; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if ((i) % 3 == 0) Console.WriteLine("------+-------+-------+-------+");
                Console.ForegroundColor = ConsoleColor.Red;
                if ((i) % 3 != 1)
                {
                    Console.Write("| | " + i + " | ");
                }
                else
                {
                    Console.Write((int)((i / 3) + 1) + " | " + i + " | ");
                }
                for (int j = 0; j < 9; j++)
                {
                    if (board[i, j]/10==1)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(board[i, j] % 10 + " ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        if ((j + 1) % 3 == 0) Console.Write("| ");
                    }
                    else
                    {
                        Console.ForegroundColor = color;
                        Console.Write(board[i, j] % 10 + " ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        if ((j + 1) % 3 == 0) Console.Write("| ");
                    }
                }
                
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("------+-------+-------+-------+");
            Console.ForegroundColor = color;
        }
    }
}