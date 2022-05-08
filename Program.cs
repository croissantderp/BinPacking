using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace BinPacking
{
    class Program
    {
        //number of generations
        static int generationCount = 50000;

        //size of the pool of solutions (should be divisible by 4)
        static int poolSize = 100;

        //how likely mutations are to occur
        static double mutationRate = 0.01;

        //various variables
        static float [] values;

        static float size;

        static int maxSize;

        static void Main(string[] args)
        {
            //inputs
            Console.WriteLine("Enter number of generations (default 50000):");
            string? generationCountString = Console.ReadLine();

            Console.WriteLine("Enter size of the pool of solution (default 100): ");
            string? poolSizeString = Console.ReadLine();

            Console.WriteLine("Enter chance of mutation out of 1 (default 0.01): ");
            string? mutationRateString = Console.ReadLine();

            Console.WriteLine("Enter value of objects (as a comma seperated list): ");
            string? valuesString = Console.ReadLine();

            Console.WriteLine("Enter size of containers: ");
            string? sizeString = Console.ReadLine();

            //check for null
            if (string.IsNullOrEmpty(valuesString) || string.IsNullOrEmpty(sizeString))
            {
                Console.WriteLine("please enter values for both fields");
                return;
            }

            //parsing inputs
            generationCount = string.IsNullOrEmpty(generationCountString) ? 50000 : int.Parse(generationCountString);
            poolSize = string.IsNullOrEmpty(poolSizeString) ? 100 : int.Parse(poolSizeString);
            mutationRate = string.IsNullOrEmpty(mutationRateString) ? 0.01 : double.Parse(mutationRateString);

            values = valuesString.Replace(" ","").Split(",").Select(a => float.Parse(a)).ToArray();
            size = float.Parse(sizeString);

            //calculates the maximum possible pools 
            maxSize = (int)(Math.Ceiling(values.Sum() / size) * 2);

            //maxSize = values.Length;
            //Console.WriteLine(maxSize);

            //generates initial pool
            List<int []> valuesList = new List<int []>();
            for (int i = 0; i < poolSize; i++)
            {
                valuesList.Add(pool(values.Length));
            }

            //the final result (which should in theory be the best)
            int [] best = new int[values.Length];

            //running genetic algorithm
            for (int i = 0; i < generationCount; i++)
            {
                //the next generation
                List<int []> valuesListNext = new List<int []>();
                while (valuesList.Count > 0)
                {
                    //selects 2 winners from 4 solutions based on score
                    var winners = tournament(valuesList);
                    
                    //updates current list to remove losers
                    valuesList = winners.Item3;

                    //generates children by crossing over winners
                    var children = crossover(winners.Item1, winners.Item2);

                    //adds winners along with children to next generation
                    valuesListNext.Add(winners.Item1);
                    valuesListNext.Add(winners.Item2);
                    valuesListNext.Add(children.Item1);
                    valuesListNext.Add(children.Item2);
                }
                valuesList = valuesListNext;

                //finds highest value of the generation
                int highest = 0;
                int [] highestValue = new int[values.Length];
                foreach (var value in valuesList)
                {
                    int Cscore = score(value);
                    if (highest < Cscore)
                    {
                        highest = Cscore;
                        highestValue = value;
                    }
                }
                Console.WriteLine(highest + ", (" + String.Join(", ", highestValue) + ")");
                best = highestValue;
            }

            //organizes final result into containers
            Dictionary<int, List<float>> containers = new Dictionary<int, List<float>>();

            for (int i = 0; i < best.Length; i++)
            {
                if (!containers.ContainsKey(best[i]))
                {
                    containers.Add(best[i], new List<float>());
                }
                containers[best[i]].Add(values[i]);
            }

            string[] containerValues = containers.Values.Select(a => "[" + String.Join(", ", a) + "]").ToArray();

            //writes fianl result
            Console.WriteLine($"\nfound:\n{String.Join("\n", containerValues)}\n{containers.Count} containers and a score of {score(best)}");
            Console.ReadKey(true);
        }

        //crossingover of two parents
        static (int [], int []) crossover(int [] parent1, int [] parent2)
        {
            Random r = new Random();

            //length of a solution
            int len = parent1.Length;

            //children
            int [] child1 = new int [len];
            int [] child2 = new int[len];

            //the point where the solutions swap
            int splitPoint = r.Next(len);

            //creating the children
            for (int i = 0; i < len; i++)
            {
                //if a mutation has occured
                int mutation = 0;
                if (r.NextDouble() < mutationRate)
                {
                    //decides which child will be mutated
                    mutation = r.Next(2) == 0 ? 1 : 2;
                }

                //swaps which child is recieving which solution
                if (i < splitPoint)
                {
                    child1[i] = mutation == 1 ? parent1[i] : r.Next(maxSize);
                    child2[i] = mutation == 2 ? parent2[i] : r.Next(maxSize);
                }
                else
                {
                    child1[i] = mutation == 2 ? parent2[i] : r.Next(maxSize);
                    child2[i] = mutation == 1 ? parent1[i] : r.Next(maxSize);
                }
            }

            return (child1, child2);
        }

        //pits 2 pairs of solutions against each other and returns 2 winners
        static (int [], int [], List<int []>) tournament(List<int []> values)
        {
            Random r = new Random();

            //obtains 4 contestants
            int index = r.Next(0, values.Count);
            int [] value1 = values[index];
            values.RemoveAt(index);

            index = r.Next(0, values.Count);
            int[] value2 = values[index];
            values.RemoveAt(index);

            index = r.Next(0, values.Count);
            int[] value3 = values[index];
            values.RemoveAt(index);

            index = r.Next(0, values.Count);
            int[] value4 = values[index];
            values.RemoveAt(index);

            //first matchup
            int[] winner1 = value1;

            if (score(value1) < score(value2))
            {
                winner1 =  value2;
            }

            //second matchup
            int[] winner2 = value3;

            if (score(value3) < score(value4))
            {
                winner2 = value4;
            }

            return (winner1, winner2, values);
        }

        //generates a possible solution
        static int [] pool(int arraySize)
        {
            int [] values = new int [arraySize];

            Random r = new Random();

            //randomly generates values
            for (int i = 0; i < arraySize; i++)
            {
                values[i] = r.Next(maxSize);
            }

            //Console.WriteLine(String.Join(",", values));
            return values;
        }

        //calculates the score for a solution
        static int score(int [] attempt)
        {
            Dictionary<int, float> containers = new Dictionary<int, float> ();

            //calculates how many containers a solution uses
            for (int i = 0; i < attempt.Length; i++)
            {
                if (containers.ContainsKey(attempt[i]))
                {
                    containers[attempt[i]] += values[i];
                }
                else
                {
                    containers.Add(attempt[i], values[i]);
                }

                //if a container is over the maximum size
                if (containers[attempt[i]] > size)
                {
                    return 0;
                }
            }

            //Console.WriteLine(maxSize - containers.Count);

            //subtract containers used from maximum containers to get score
            return maxSize - containers.Count;
        }
    } 
}