using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Simplex
{
    class Program
    {
        static void menu()
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("1 ... SIMPLEX");
            Console.WriteLine("2 ... TEST");
            Console.WriteLine("4 ... IZHOD");
            Console.WriteLine("==========================================");
            Console.WriteLine("Izberi");
        }

        static public bool InitSimplex(List<List<double>> A, List<double> b, List<double> c, int n, int m, out List<int> N, out List<int> B)
        {
            N = new List<int>(); 
            B = new List<int>(); 

            if (b.Min() >= 0)
            {
                for(int i = 0; i < n; i++)
                    N.Add(i); //neosnovne spremenljivke ... z močjo n

                for (int i = 0; i < m; i++)
                    B.Add(n+i); //osnovne spremenljivke ... z močjo m
            }
            else
            {
                N = B = null;
                Console.WriteLine("ERROR (Negativna vrednost v vektorju b)");
                return false;
            }

            return true;
        }

        static public List<List<double>> setListToZero(int n, int m)
        {
            List<List<double>> a = new List<List<double>>();

            for (int i = 0; i < n; i++)
            {
                List<double> temp = new List<double>();

                for (int j = 0; j < m; j++)
                    temp.Add(0);

                a.Add(temp);
            }

            return a;
        }

        static public bool Pivot(List<List<double>> A, List<double> b, List<double> c, List<int> N, List<int> B, double V, int l, int e,
            out List<List<double>> n_A, out List<double> n_b, out List<double> n_c, out List<int> n_N, out List<int> n_B, out double n_V)
        {
            n_b = new List<double>(new double [b.Count]);
            n_c = new List<double>(new double[b.Count]);
            n_A = setListToZero(A.Count, A[0].Count);

            n_N = N;
            n_B = B;

            n_b[e] = b[l] / A[l][e];

            foreach (var el in N)
            {
                if (el == e) continue;
                n_A[e][el] = A[l][el] / A[l][e];
            }
            n_A[e][l] = 1 / A[l][e];


            foreach (var elb in B)
            {
                if (elb == l) continue;
                n_b[elb] = b[elb] - A[elb][e] * n_b[e];

                foreach (var eln in N)
                {
                    if (eln == e) continue;
                    n_A[elb][eln] = A[elb][eln] - A[elb][e] * n_A[e][eln];
                }
                n_A[elb][l] = -1 * A[elb][e] * n_A[e][l];

            }
            n_V = V + c[e] * n_b[e];

            foreach(var el in N)
            {
                if (el == e) continue;
                n_c[el] = c[el] - c[e] * n_A[e][el];
            }
            n_c[l] = -1 * c[e] * n_A[e][l];

            n_N.Add(l);
            n_N.Remove(e);

            n_B.Add(e);
            n_B.Remove(l);
            n_B.Sort();

            return true;
        }

        static public bool ExistBetterSolution (List<double> c, List<int> N, out int k)
        {  
            foreach (var el in N)
            {
                if (c[el] > 0)
                {
                    k=el;
                    return true;
                }

            }
            k = -1;
            return false;
        }

        static public bool Simplex(List<List<double>> A, List<double> b, List<double> c, int n, int m)
        {
            List<int> N, B;
            double V = 0;

            if(!InitSimplex(A, b, c, n, m, out N, out B))
                return false;

            List<double> delta = new List<double>();
            List<int> index_b = new List<int>();
            int l; //index najmanjšega elementa v delti

            int e = -1;
            while (ExistBetterSolution(c, N, out e))
            {
                foreach (var j in B)
                {
                    if (A[j][e] > 0)
                    {
                        delta.Add(b[j] / A[j][e]);
                        index_b.Add(j);
                    }
                    else
                    {
                        delta.Add(double.MaxValue); //infinity
                        index_b.Add(j);
                    }
                }

                l = index_b[delta.IndexOf(delta.Min())];
                index_b.Clear();

                if (delta[delta.IndexOf(delta.Min())] == double.MaxValue)
                {
                    Console.WriteLine("ERROR (Neomejen program)");
                    return false;
                }
                delta.Clear();


                if (!Pivot(A, b, c, N, B, V, l, e, out A, out b, out c, out N, out B, out V))
                    return false;

                //printMatrix(A);
                //printVector(b);
                //printVector(c);
                //printVectorInt(N);
                //printVectorInt(B);
                //Console.WriteLine(V);
            }

            Dictionary<string, double> x = new Dictionary<string, double>();

            for (int i = 0; i < n; i++)
            {
                if (B.Contains(i)) x.Add("x" + i, b[i]);
                else x.Add("x" + i, 0);
            }

            //Print result
            //foreach(var el in x)
            //    Console.WriteLine(el.Key + " = " + el.Value);

            //Console.WriteLine("z = " + V);

            return true;
        }

        public static void printVector (List<double> b)
        {
            foreach(var el in b)
                Console.Write(el + " ");

            Console.WriteLine("");

        }

        public static void printVectorInt(List<int> b)
        {
            foreach (var el in b)
                Console.WriteLine(el + " ");

            Console.WriteLine("");

        }

        public static void printMatrix(List<List<double>> A)
        {
            foreach (var line in A)
            {
                foreach (var num in line)
                    Console.Write(num + " ");

                Console.WriteLine("");
            }
            Console.WriteLine("");

        }

        public static double GetRandomNumber(int minimum, int maximum)
        {
            Random random = new Random();
            return random.Next(minimum, maximum); //random.NextDouble() * (maximum - minimum) + minimum;
        }


        public static void GenerateMatrix(int i, out List<List<double>> A, out List<double> b, out List<double> c)
        {
            A = new List<List<double>>();

            b = new List<double>();
            c = new List<double>();

            for (int j = 0; j < i; j++)
            {
                List<double> temp = new List<double>();

                for (int k = 0; k < i; k++)
                {
                    temp.Add(GetRandomNumber(-20, 30));

                    b.Add(GetRandomNumber(0, 20));
                    c.Add(GetRandomNumber(0, 20));
                }

                A.Add(temp);
            }
        }

        public static void TestSimplex(int maxSize= 100, int repNum= 8)
        {
            List<List<double>> A = new List<List<double>>();

            List<double> b = new List<double>();
            List<double> c = new List<double>();

            double time = 0;

            double min = 9999999;
            double max = -1;

            for (int i = 2; i < maxSize; i++)
            {
                for (int j = 1; j < repNum; j++)
                {

                    GenerateMatrix(i*2, out A, out b, out c);


                    //printMatrix(A);
                    //printVector(b);
                    //printVector(c);

                    //Console.WriteLine("Size: " + i * 2);

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    Simplex(A, b, c, i, i);
                    sw.Stop();

                    //Console.WriteLine("\nElapsed={0}", sw.ElapsedMilliseconds);

                    time += sw.ElapsedMilliseconds;

                    if (sw.ElapsedMilliseconds < min) min = sw.ElapsedMilliseconds;
                    if (sw.ElapsedMilliseconds > max) max = sw.ElapsedMilliseconds;
                }

                Console.WriteLine(max);
                time = 0;
                min = 999999999;
                max = -1;

            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Simplex!");
            Console.WriteLine("Izberite želeno akcijo!");


            string matrix = "";

            int n = -1, m = -1;
            List<List<double>> A = new List<List<double>>();
            List<double> b = new List<double>();
            List<double> c = new List<double>();

            //Read all data
            var lines = File.ReadLines("lprogram.txt");
            int countEmpty = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    countEmpty++;
                    continue;
                }

                if (countEmpty == 0) {
                    n = int.Parse(line.Split(" ")[0]);
                    m = int.Parse(line.Split(" ")[1]);
                }
                else if (countEmpty == 1)
                {
                    List<double> temp = new List<double>();

                    foreach (var el in line.Split(" "))
                    {
                        temp.Add(double.Parse(el));
                    }

                    A.Add(temp);
                }
                else if (countEmpty == 2)
                {
                    foreach (var el in line.Split(" "))
                    {
                        b.Add(double.Parse(el));
                    }
                }
                else if (countEmpty == 3)
                {
                    foreach (var el in line.Split(" "))
                    {
                        c.Add(double.Parse(el));
                    }
                }

            }

            //Print matrix
            //foreach (var line in A)
            //{
            //    foreach (var num in line)
            //    {
            //        Console.Write(num + " ");
            //    }
            //    Console.WriteLine("");
            //}


            bool exit = false;
            do
            {
                menu();
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    Simplex(A, b, c, n , m);
                }
                else if (choice == "2")
                {
                    TestSimplex();
                }
                else
                {
                    exit = true;
                }
            }
            while (!exit);
        }
    }
}
