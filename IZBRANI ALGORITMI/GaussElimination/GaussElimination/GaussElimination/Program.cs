using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GaussElimination
{
    class Program
    {

        static void GaussElimination(double[,] A, int n)
        {

            for (int k = 0; k < n; k++)
            {
                //Find pivot
                int p = k;
                double minPivot = A[0,0];
                for (int i = k; i < n; i++)
                {
                    if (Math.Abs(A[i,k]) < Math.Abs(minPivot))
                    {
                        minPivot = A[i,k];
                        p = i;
                    }
                }

                if (minPivot == 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Ni rešitve! (rešitev ne obstaja)");
                    return;
                }


                //Swap rows
                if (p != k)
                {
                    for (int i = k; i < n + 1; i++)
                    {
                        double temp = A[p, i];
                        A[p, i] = A[k, i];
                        A[k, i] = temp;
                    }
                }

                //Elimination
                for (int i = k + 1; i < n; i++)
                {
                    double m = A[i,k] / A[k,k];

                    for (int j = k; j < n + 1; j++)
                    {
                        A[i, j] -= m * A[k, j];
                    }
                }

            }

            if (A[n-1,n-1] == 0)
            {
                if (A[n-1, n] == 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Ni rešitve! (neskončno rešitev)");
                    return;
                }

                Console.WriteLine("");
                Console.WriteLine("Ni rešitve! (rešitev ne obstaja)");
                return;
            }

            double[] x = new double[n];

            //Back substitution
            for (int k = n - 1; k >= 0; k--)
            {
                double sum = A[k, n];
                for (int j = k + 1; j < n; j++)
                {
                    sum -= A[k, j] * x[j];
                }
                x[k] = sum / A[k, k];
            }

            Console.Write(Environment.NewLine);

            int index = 1;
            foreach(double _x in x)
            {
                Console.WriteLine("x" + index + ": " + _x.ToString("N2"));
                index++;
            }

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Gaussova eliminacija!");

            string matrix = "";
            int matrixSize;

            //Read matrix from file
            try
            {
                using (var sr = new StreamReader("sistem.txt"))
                {
                    matrix = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: \n" + e.Message);
            }

            matrixSize = int.Parse(matrix.Split(System.Environment.NewLine)[0].ToString());
            string[] arr = matrix.Replace(System.Environment.NewLine, " ").Split(" ");

            //Save into 2D array
            double[,] array = new double[matrixSize + 1, matrixSize + 1];
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j <= matrixSize; j++)
                {
                    array[i, j] = double.Parse(arr[(i * (matrixSize + 1) + j) +1].ToString());
                }
            }

            //Print matrix
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j <= matrixSize; j++)
                {
                    Console.Write(array[i, j] + " ");
                }
                Console.Write(Environment.NewLine);
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();

            GaussElimination(array, matrixSize);

            sw.Stop();


            Console.WriteLine("\nElapsed={0}", sw.Elapsed);
        }
    }
}
