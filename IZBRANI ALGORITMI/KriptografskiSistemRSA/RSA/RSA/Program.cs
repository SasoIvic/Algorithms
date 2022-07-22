using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace RSA
{
    public class BinWriter
    {
        private BinaryWriter binaryWriter { set; get; }
        private string bitString { set; get; }

        private dynamic _file;

        public BinWriter(string file)
        {
            //this.binaryWriter = new BinaryWriter(File.Open(file, FileMode.Create));
            _file = File.Open(file, FileMode.Create);

        }

        public void writeNum(BigInteger num, bool addZerosToCompleteByte = false)
        {
            string binary = Program.bigIntToBinaryString(num);

            if (addZerosToCompleteByte)
                binary = this.addLeadingZeros(binary, 128);

            foreach (char bit in binary)
                this.writeBit(bit);

        }

        public void writeBits(string x)
        {
            for (int i = 0; i < x.Length; i++)
                writeBit(x[i]);
        }
        public void writeBit(char x)
        {
            this.bitString += x;
            if (this.bitString.Length > 7)
                writeByte();

        }
        public void writeByte()
        {
            BinaryWriter writer = new BinaryWriter(_file);
            writer.Write(Convert.ToByte(this.bitString, 2));
            this.bitString = "";

        }
        public void writeChar(char c)
        {
            BinaryWriter writer = new BinaryWriter(_file);
            writer.Write(c);
        }
        public void finishByte()
        {
            if (this.bitString.Length > 0) {
                int manjkajoce = 8 - this.bitString.Length;
                for (int i = 0; i < manjkajoce; i++)
                    writeBit('0');
            }
        }

        public String addLeadingZeros(String x, int dolzina)
        {
            int zeroNum = dolzina - x.Length;
            for (int i = 0; i < zeroNum; i++)
                x = 0 + x;

            return x;
        }

        public void CloseFile()
        {
            this._file.Close();
        }
    }

    public class BinReader
    {
        public BinaryReader binaryReader { set; get; }
        private string bitStringRead { set; get; }
        private int currPos { set; get; }

        private dynamic _file;

        public BinReader(string file)
        {
            _file = File.Open(file, FileMode.Open);
            this.currPos = 8;
            this.binaryReader = new BinaryReader(_file);
        }

        public string readBits(int st)
        {
            try
            {
                string tmp = "";
                for (int i = 0; i < st; i++)
                    tmp += readBit();

                return tmp;
            }
            catch
            {
                return "";
            }
        }
        public char readBit()
        {
            try
            {
                if (this.currPos > 7)
                    readByte();

                char tmp = this.bitStringRead[this.currPos];
                this.currPos++;
                return tmp;
            }
            catch
            {
                return ' ';
            }
        }
        public String readByte()
        {
            try
            {
                this.bitStringRead = addLeadingZeros(Convert.ToString(binaryReader.ReadByte(), 2), 8);
                this.currPos = 0;
                return bitStringRead;
            }
            catch
            {
                return "";
            }
        }
        public char readChar()
        {
            try
            {
                char c = (char)binaryReader.ReadByte();
                return c;
            }
            catch
            {
                return ' ';
            }
        }
        public String addLeadingZeros(String x, int dolzina)
        {
            int zeroNum = dolzina - x.Length;
            for (int i = 0; i < zeroNum; i++)
                x = 0 + x;

            return x;
        }

        public void CloseFile()
        {
            this._file.Close();
        }
    }

    public class Program
    {
        static BigInteger _prev = 1;

        static void menu()
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("1 ... GENERIRAJ KLJUČE");
            Console.WriteLine("2 ... KODIRANJE DATOTEKE");
            Console.WriteLine("3 ... DEKODIRANJE DATOTEKE");
            Console.WriteLine("4 ... IZHOD");
            Console.WriteLine("==========================================");
            Console.WriteLine("Izberi");
        }

        static BigInteger randomGenerator()
        {
            BigInteger m = (BigInteger)Math.Pow(2, 32);
            BigInteger a = 69069;
            BigInteger b = 0;

            _prev = ((a * _prev) + b) % m;
            return _prev;
        }

        static BigInteger random(BigInteger a, BigInteger b)
        {
            return a + randomGenerator() % (b - a + 1);
        }

        public static string bigIntToBinaryString(BigInteger bi)
        {
            var bytes = bi.ToByteArray(); //Convert.ToString(b, 2);

            var idx = bytes.Length - 1;
            var base2 = new StringBuilder(bytes.Length * 8);

            var binary = Convert.ToString(bytes[idx], 2);

            if (binary[0] != '0' && bi.Sign == 1)
            {
                base2.Append('0');
            }

            base2.Append(binary);

            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            return base2.ToString();
        }

        static BigInteger modularExponentiation(BigInteger a, BigInteger b, BigInteger n)
        {
            BigInteger d = 1;

            string _b = bigIntToBinaryString(b);

            for (int i = 0; i <= _b.Length - 1; i++)
            {
                d = (d * d) % n;

                if (_b[i] == '1')
                    d = (d * a) % n;
            }

            return d;
        }

        static int millerRabinAlgorithm(ref BigInteger potencialPrime, int certainty)
        {
            if (potencialPrime == 2 || potencialPrime == 3) return 0;

            if (potencialPrime < 2 || potencialPrime % 2 == 0) return -1;

            BigInteger d = potencialPrime - 1;
            int k = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                k += 1;
            }

            for (int j = 1; j < certainty; j++)
            {
                BigInteger a = random(2, potencialPrime - 2);

                BigInteger x = modularExponentiation(a, d, potencialPrime);

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

        static BigInteger randNum(BigInteger lowerLimit, BigInteger upperLimit, int s)
        {
            BigInteger rnd = random(lowerLimit, upperLimit);
            int isPrime = -1;

            while (isPrime == -1)
            {
                if (rnd % 2 == 0) rnd += 1;

                isPrime = millerRabinAlgorithm(ref rnd, s);

                if (isPrime == -1)
                    rnd += 2;

                //če število preseže limit za željen bitni zapis, poiščemo novo število
                if (rnd > upperLimit)
                    random(lowerLimit, upperLimit);
            }

            return rnd;
        }

        static BigInteger GCD(BigInteger a, BigInteger b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            //or
            return a | b;
        }

        static void extendedEuclid(BigInteger a, BigInteger b, ref BigInteger d, ref BigInteger x, ref BigInteger y)
        {
            BigInteger r;
            BigInteger _d = d;
            BigInteger _x = x;
            BigInteger _y = y;

            if (b == 0)
            {
                d = a;
                x = 1;
                y = 0;
            }
            else
            {
                if (a <= 0) r = a + b;
                else r = a % b;
                extendedEuclid(b, r, ref _d, ref _x, ref _y);
                d = _d;
                x = _y;
                y = _x - (a / b) * _y;
            }
        }

        static void modularLinearEqSolver(BigInteger a, BigInteger b, BigInteger n, ref BigInteger d, ref BigInteger x, ref BigInteger y)
        {
            extendedEuclid(a, b, ref d, ref x, ref y);

            if (b % d == 0)
            {
                BigInteger _x = x * (b / d) % n;

                for (long i = 0; i <= d - 1; i++)
                {
                    Console.WriteLine(_x + i * (n / d) % n);
                }
            }
            else
            {
                Console.WriteLine("Rešitev ne obstaja");

            }
        }

        public static string BinToDec(string value)
        {
            // BigInteger can be found in the System.Numerics dll
            BigInteger res = 0;

            // I'm totally skipping error handling here
            foreach (char c in value)
            {
                res <<= 1;
                res += c == '1' ? 1 : 0;
            }

            return res.ToString();
        }

        static void Main(string[] args)
        {

            Console.WriteLine("RSA kodirnik in dekodirnik!");
            Console.WriteLine("Izberite želeno akcijo!");

            BinReader reader;
            BinWriter writer;

            bool exit = false;
            int ex = 1;
            do
            {
                menu();
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    //zanesljivost Miller-Rabin
                    int s = 20;

                    Console.Clear();
                    Console.WriteLine("IZBRALI STE GENERATOR KLJUČEV RSA");
                    Console.WriteLine("Koliko bitov naj ima praštevilo?");
                    ex = Int32.Parse(Console.ReadLine());

                    //for (int i = 100; i < 1000; i += 100)
                    {

                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        //zgornja in spodja meja
                        BigInteger upperLimit = (BigInteger)(Math.Pow(2, ex) - 1);
                        BigInteger lowerLimit = (BigInteger)(Math.Pow(2, ex - 1));

                        //naključni praštevili
                        BigInteger p = randNum(lowerLimit, upperLimit, s);
                        BigInteger q = randNum(lowerLimit, upperLimit, s);

                        while (p == q)
                        {
                            q = randNum(lowerLimit, upperLimit, s);
                            _prev++;
                        }

                        Console.WriteLine("Naključno praštevilo 1: " + p);
                        Console.WriteLine("Naključno praštevilo 2: " + q);

                        BigInteger n = p * q;

                        //euler
                        BigInteger fn = (p - 1) * (q - 1);

                        //naključno manjše liho število 
                        BigInteger e = random(1, fn);

                        while (GCD(e, fn) != 1 || e % 2 == 0)
                        {
                            e = random(1, fn);
                            if (e % 2 == 0) e += 1;
                        }

                        //skriti eksponent
                        BigInteger d = 1, x = 1, y = 1;
                        modularLinearEqSolver(e, fn, n, ref d, ref x, ref y);

                        if (x <= 0) x += fn;

                        sw.Stop();

                        //Console.WriteLine("Coefficient for smaller integer (x): " + x);

                        //zapis javnega ključa
                        writer = new BinWriter("publickey.txt");

                        int nLength = bigIntToBinaryString(n).Length;

                        //dolžina posameznaga dela ključa
                        writer.writeNum(bigIntToBinaryString(e).Length, true);
                        writer.writeNum(nLength, true);

                        writer.writeNum(e);
                        writer.writeNum(n);
                        writer.finishByte();
                        writer.CloseFile();

                        //zapis privatnega ključa
                        writer = new BinWriter("privatekey.txt");

                        //dolžina posameznaga dela ključa
                        writer.writeNum(bigIntToBinaryString(x).Length, true);
                        writer.writeNum(nLength, true);

                        writer.writeNum(x);
                        writer.writeNum(n);
                        writer.finishByte();
                        writer.CloseFile();

                        Console.WriteLine("\nElapsed={0}", sw.Elapsed);
                    }
                }
                else if (choice == "2")
                {
                    Console.Clear();
                    Console.WriteLine("KODIRANJE");

                    //preberi PublicKey
                    reader = new BinReader("publickey.txt");

                    int eLength = int.Parse(BinToDec(reader.readBits(128)));
                    int nLength = int.Parse(BinToDec(reader.readBits(128)));

                    BigInteger e = BigInteger.Parse(BinToDec(reader.readBits(eLength)));
                    BigInteger n = BigInteger.Parse(BinToDec(reader.readBits(nLength)));

                    Console.WriteLine("PublicKey = (" + e + "," + n + ")");

                    int readLength = (int)(BigInteger.Log10(n) / BigInteger.Log10(2)); // = log2(n)

                    reader.CloseFile();


                    Stopwatch sw = new Stopwatch();
                    BigInteger codedBits;
                    BigInteger bits;
                    string read, write, temp;

                    writer = new BinWriter("enc.txt");
                    reader = new BinReader("msg.txt");

                    sw.Start();

                    //ugotovimo s koliko biti je zapisan zadnji prebran nabor
                    long bitsCounter = 0;
                    while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                    {
                        read = reader.readBits(8).Trim(' ');
                        bitsCounter += read.Length;
                    }

                    reader.CloseFile();

                    int lastBitsCount = (int)(bitsCounter % readLength);

                    //zapišem število bitov v zadnjem naboru
                    writer.writeNum(lastBitsCount, true);
                    Console.WriteLine("Last Bits Count: " + lastBitsCount);

                    reader = new BinReader("msg.txt");

                    while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                    {
                        read = reader.readBits(readLength).Trim(' ');
                        bits = BigInteger.Parse(BinToDec(read));
                        codedBits = modularExponentiation(bits, e, n);

                        temp = bigIntToBinaryString(codedBits);
                        if (temp.Length > readLength + 1)
                            temp = temp.Remove(0, 1);

                        write = writer.addLeadingZeros(temp, readLength + 1);
                        writer.writeBits(write);

                        //Console.WriteLine("read msg: " + read + " decimal: " + bits + " coded: " + codedBits + " write: " + write);
                        //Console.WriteLine("read: " + read + " write: " + write);
                        //Console.WriteLine("read: " + read + " write: " + write);
                    }

                    read = reader.readBits(readLength).Trim(' ');

                    if (!string.IsNullOrWhiteSpace(read))
                    {
                        bits = BigInteger.Parse(BinToDec(read));
                        codedBits = modularExponentiation(bits, e, n);

                        temp = bigIntToBinaryString(codedBits);
                        if (temp.Length > readLength + 1)
                            temp = temp.Remove(0, 1);

                        write = writer.addLeadingZeros(temp, readLength + 1);
                        writer.writeBits(write);

                        //Console.WriteLine("read msg: " + read + " decimal: " + bits + " coded: " + codedBits + " write: " + write);
                        //Console.WriteLine("read: " + read + " write: " + write);
                        //Console.WriteLine("read: " + read + " write: " + write);
                    }

                    writer.finishByte();

                    sw.Stop();

                    writer.CloseFile();
                    reader.CloseFile();

                    Console.WriteLine("Coded message saved in enc.txt");
                    Console.WriteLine("\nElapsed={0}", sw.Elapsed);
                }
                else if (choice == "3")
                {
                    Console.Clear();
                    Console.WriteLine("DEKODIRANJE");

                    //preberi PrivateKey
                    reader = new BinReader("privatekey.txt");

                    int dLength = int.Parse(BinToDec(reader.readBits(128)));
                    int nLength = int.Parse(BinToDec(reader.readBits(128)));

                    BigInteger d = BigInteger.Parse(BinToDec(reader.readBits(dLength)));
                    BigInteger n = BigInteger.Parse(BinToDec(reader.readBits(nLength)));

                    Console.WriteLine("PrivateKey = (" + d + "," + n + ")");

                    int readLength = (int)(BigInteger.Log10(n) / BigInteger.Log10(2)); // = log2(n)

                    reader.CloseFile();

                    Stopwatch sw = new Stopwatch();
                    BigInteger codedBits;
                    BigInteger bits;
                    string temp;
                    string read = "";

                    writer = new BinWriter("dec.txt");
                    reader = new BinReader("enc.txt");

                    int lastBitsLength = int.Parse(BinToDec(reader.readBits(128)));

                    sw.Start();

                    while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                    { 
                        char bit = reader.readBit();
                        read += bit;

                        if (read.Length == readLength + 1) {

                            codedBits = BigInteger.Parse(BinToDec(read));
                            bits = modularExponentiation(codedBits, d, n);

                            //odstranim odvečno ničlo (ki pri BigInteger pomeni predznak +)
                            temp = bigIntToBinaryString(bits);
                            if (temp.Length > readLength)
                                temp = temp.Remove(0, 1);

                            //peek
                            var pos = reader.binaryReader.BaseStream.Position;

                            try
                            {
                                reader.binaryReader.ReadByte();

                                if (reader.binaryReader.BaseStream.Position >= reader.binaryReader.BaseStream.Length)
                                    break;
                            }
                            catch
                            {
                                if (reader.binaryReader.BaseStream.Position >= reader.binaryReader.BaseStream.Length)
                                    break;
                            }

                            writer.writeBits(writer.addLeadingZeros(temp, readLength));
                            reader.binaryReader.BaseStream.Position = pos;


                            //Console.WriteLine("read: " + read + " coded: " + codedBits + " msg: " + bits + " write: " + temp);
                            //Console.WriteLine("read: " + read + " write: " + temp);
                            //Console.WriteLine("read: " + read + " write: " + temp);

                            read = "";
                        }
                    }

                    //preberi še zadnje bite
                    while (read.Length < readLength + 1)
                    {
                        char bit = reader.readBit();
                        read += bit;
                    }

                    codedBits = BigInteger.Parse(BinToDec(read));
                    bits = modularExponentiation(codedBits, d, n);

                    temp = bigIntToBinaryString(bits);
                    if (temp.Length > lastBitsLength)
                        temp = temp.Remove(0, 1);

                    writer.writeBits(writer.addLeadingZeros(temp, lastBitsLength));

                    //Console.WriteLine("read: " + read + " coded: " + codedBits + " msg: " + bits + " write: " + temp);
                    //Console.WriteLine("read: " + read + " write: " + temp);
                    //Console.WriteLine("read: " + read + " write: " + temp);

                    writer.finishByte();

                    sw.Stop();

                    writer.CloseFile();
                    reader.CloseFile();

                    Console.WriteLine("Decoded message saved in dec.txt");
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