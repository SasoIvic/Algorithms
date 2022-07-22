using System;
using System.Diagnostics;

namespace PrimeNumbers
{
    class Program
    {
        static ulong _prev = 1;

        static void menu()
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("1 ... GENERATOR: NAIVNI ALGORITEM");
            Console.WriteLine("2 ... GENERATOR: MILLER-RABINOV TEST");
            Console.WriteLine("3 ... TEST: NAIVNI ALGORITEM");
            Console.WriteLine("4 ... TEST: MILLER-RABINOV TEST");
            Console.WriteLine("5 ... IZHOD");
            Console.WriteLine("==========================================");
            Console.WriteLine("Izberi");
        }

        static ulong randomGenerator()
        {
            ulong m = (ulong)Math.Pow(2, 32);
            ulong a = 69069;
            ulong b = 0;

            _prev = ((a * _prev) + b) % m;
            return _prev;
        }

        static ulong random(ulong a, ulong b)
        {
            return a + randomGenerator() % (b - a + 1);
        }

        static int nativeAlgorithm(ref ulong potencialPrime, ulong upperLimit, ulong lowerLimit)
        {
            bool isPrime = false;

            if (potencialPrime == 2 || potencialPrime == 3) return 1;

            if (potencialPrime % 2 == 0) potencialPrime += 1;

            while (!isPrime)
            {
                double divisor = 3.0;

                //če je potencialPrime / divisor % 1 == 0, potem smo našli deliteja in ni praštevilo
                while (potencialPrime / divisor % 1 != 0 && divisor <= Math.Sqrt(potencialPrime))
                    divisor += 2;

                //če je divisor > √p, smo prišli skozi vse potencialne delitelje
                //ker delitelja nismo našli, smo našli praštevilo
                if (divisor > Math.Sqrt(potencialPrime))
                    return 1;

                //če ni praštevilo, ga povečamo za 2
                potencialPrime += 2;

                //če število preseže limit za željen bitni zapis, poiščemo novo število
                if (upperLimit != 0 && potencialPrime > upperLimit)
                {
                    random(lowerLimit, upperLimit);

                    if (potencialPrime == 2 || potencialPrime == 3) return 1;
                    if (potencialPrime % 2 == 0) potencialPrime += 1;
                }
            }

            return 0;
        }

        static ulong modularExponentiation(ulong a, ulong b, ulong n)
        {
            ulong d = 1;
            string _b = Convert.ToString((long)b, 2);

            for(int i = 0; i <= _b.Length - 1; i++)
            {
                d = (d * d) % n;

                if (_b[i] == '1')
                    d = (d * a) % n;
            }

            return d;
        }

        static int millerRabinAlgorithm(ref ulong potencialPrime, int certainty)
        {
            if (potencialPrime == 2 || potencialPrime == 3) return 0;

            if (potencialPrime < 2 || potencialPrime % 2 == 0) return -1;

            ulong d = potencialPrime - 1;
            int k = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                k += 1;
            }

            for (int j = 1; j < certainty; j++)
            {
                ulong a = random(2, potencialPrime - 2);

                ulong x = modularExponentiation(a, d, potencialPrime);
                //ulong x = (ulong)(Math.Pow(a, d) % potencialPrime);

                if (x != 1)
                {
                    for (int i = 0; i < k - 1; i++)
                    {
                        if (x == potencialPrime - 1)
                            break;

                        x = modularExponentiation(x, 2, potencialPrime);
                    }

                    if (x != potencialPrime - 1)
                        return -1;
                }
 
            }

            return 1;

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Praštevila!");
            Console.WriteLine("Izberite algoritem!");

            bool exit = false;
            int n = 1;
            do
            {
                menu();
                string choice = Console.ReadLine();

                if(choice == "1")
                {
                    Console.Clear();
                    Console.WriteLine("IZBRALI STE GENERATOR Z NAIVNIM ALGORITMOM");
                    Console.WriteLine("Koliko bitov naj ima praštevilo?");
                    n = Int32.Parse(Console.ReadLine());

                    if (n <= 1)
                        Console.WriteLine("1 bitno praštevilo ne obstaja!");
                    else
                    {

                        //Generiramo naključno število z n biti
                        ulong upperLimit = (ulong)(Math.Pow(2, n) - 1);
                        ulong lowerLimit = (ulong)(Math.Pow(2, n - 1));
                        ulong rnd = random(lowerLimit, upperLimit);
                        Console.WriteLine("Naključno število: " + rnd);

                        Stopwatch sw = new Stopwatch();

                        sw.Start();
                        int isPrime = nativeAlgorithm(ref rnd, upperLimit, lowerLimit);
                        sw.Stop();

                        if (isPrime != 1)
                            Console.WriteLine("Praštevilo ni najdeno.");
                        else
                            Console.WriteLine("Praštevilo: " + rnd);


                        Console.WriteLine("\nElapsed={0}", sw.Elapsed);
                    }

                }
                else if (choice == "2")
                {
                    Console.Clear();
                    Console.WriteLine("IZBRALI STE GENERATOR Z MILLER-RABINOVIM TESTOM");
                    Console.WriteLine("Vnesite želeno zanesljivost: ");
                    int s = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Koliko bitov naj ima praštevilo?");
                    n = Int32.Parse(Console.ReadLine());

                    //Generiramo naključno število z n biti
                    ulong upperLimit = (ulong)(Math.Pow(2, n) - 1);
                    ulong lowerLimit = (ulong)(Math.Pow(2, n - 1));
                    ulong rnd = random(lowerLimit, upperLimit);
                    Console.WriteLine("Naključno število: " + rnd);

                    int isPrime = -1;

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    while (isPrime == -1)
                    {
                        isPrime = millerRabinAlgorithm(ref rnd, s);

                        if(isPrime == -1)
                            rnd += 2;

                        //če število preseže limit za željen bitni zapis, poiščemo novo število
                        if (rnd > upperLimit)
                            random(lowerLimit, upperLimit);
                    }
                    sw.Stop();

                    if (isPrime != -1)
                        Console.WriteLine("Praštevilo: " + rnd);


                    Console.WriteLine("\nElapsed={0}", sw.Elapsed);

                }
                else if (choice == "3")
                {
                    Console.Clear();
                    Console.WriteLine("TESTIRANJE PRAŠTEVIL Z NAIVNIM ALGORITMOM");
                    Console.WriteLine("Vnesite število: ");

                    n = Int32.Parse(Console.ReadLine());

                    ulong num = (ulong)n;

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    int isPrime = nativeAlgorithm(ref num, 0, 0);
                    sw.Stop();

                    if (num == (ulong)n)
                        Console.WriteLine("Je praštevilo");
                    else
                        Console.WriteLine("Ni praštevilo");

                    Console.WriteLine("\nElapsed={0}", sw.Elapsed);

                }
                else if (choice == "4")
                {
                    Console.Clear();
                    Console.WriteLine("TESTIRANJE PRAŠTEVIL Z MILLER-RABINOVIM TESTOM");
                    Console.WriteLine("Vnesite želeno zanesljivost: ");
                    int s = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Vnesite število: ");
                    n = Int32.Parse(Console.ReadLine());

                    ulong num = (ulong)n;

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    int isPrime = millerRabinAlgorithm(ref num, s);
                    sw.Stop();

                    if (isPrime == -1)
                        Console.WriteLine("Ni praštevilo");
                    else if (isPrime == 1)
                        Console.WriteLine("Verjetno praštevilo: " + num);
                    else
                        Console.WriteLine("Praštevilo: " + num);

                    Console.WriteLine("\nElapsed={0}", sw.Elapsed);

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
