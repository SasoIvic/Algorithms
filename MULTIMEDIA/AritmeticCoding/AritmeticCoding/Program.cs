using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AritmeticCoding
{
    public class BinWriter
    {
        private BinaryWriter binaryWriter { set; get; }
        private string bitString { set; get; }

        private dynamic _file;

        private int counter = 0;

        public BinWriter(string file)
        {
            //this.binaryWriter = new BinaryWriter(File.Open(file, FileMode.Create));
            _file = File.Open(file, FileMode.Create);

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
        public void finishByte()
        {
            int manjkajoce = 8 - this.bitString.Length;
            for (int i = 0; i < manjkajoce; i++)
                writeBit('0');
        }
        public String addLeadingZeros(String x, int dolzina)
        {
            int zeroNum = dolzina - x.Length;
            for (int i = 0; i < zeroNum; i++)
                x = '0' + x;

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
        public static FileStream filestream { set; get; }

        public BinReader(string file)
        {
            filestream = File.Open(file, FileMode.Open);
            this.binaryReader = new BinaryReader(filestream);
            this.currPos = 8;
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
        public String addLeadingZeros(String x, int dolzina)
        {
            int zeroNum = dolzina - x.Length;
            for (int i = 0; i < zeroNum; i++)
                x = 0 + x;

            return x;
        }
    }

    class Program
    {
        public class Chunk
        {
            public int frequency;
            public int lowerLimit;
            public int upperLimit;
        }


        static void Encode(string filename, string filenameOut, int numOfBits, Dictionary<string, Chunk> chunks, int maxFrequLength)
        {
            //initialization
            long lowerLimitCoder = 0;
            long upperLimitCoder = (long)Math.Pow(2, numOfBits - 1) - 1;
            long secondQuater = (upperLimitCoder + 1) / 2;
            long firstQuater = secondQuater / 2;
            long thirdQuater = firstQuater * 3;
            long comulativeFrequency = chunks.Last().Value.upperLimit;

            string output = "";
            long E3_counter = 0;


            BinWriter writer = new BinWriter(filenameOut);

            //št bitov kodirnika z 8 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(numOfBits, 2), 8))
                writer.writeBit(c);

            //št bitov za zapis frekvence z 8 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(maxFrequLength, 2), 8))
                writer.writeBit(c);

            //št znakov z 32 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(chunks.Count, 2), numOfBits))
                writer.writeBit(c);

            //dolžina zadnjega chunka z 8 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(chunks.Last().Key.Trim(' ').Length, 2), 8))
                writer.writeBit(c);

            //znaki z 32 biti in frekvence znakov z maxFrequLength biti                    
            foreach (KeyValuePair<string, Chunk> c in chunks)
            {
                writer.writeBits(writer.addLeadingZeros(c.Key.Trim(' '), numOfBits));

                foreach (char f in writer.addLeadingZeros(Convert.ToString(c.Value.frequency, 2), maxFrequLength))
                    writer.writeBit(f);
            }


            BinReader reader = new BinReader(filename);

            while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
            {
                string chunkString = reader.readBits(numOfBits);

                Chunk chunk;
                chunks.TryGetValue(chunkString, out chunk);

                long step = (upperLimitCoder - lowerLimitCoder + 1) / comulativeFrequency;
                upperLimitCoder = lowerLimitCoder + step * chunk.upperLimit - 1;
                lowerLimitCoder = lowerLimitCoder + step * chunk.lowerLimit;

                while (upperLimitCoder < secondQuater || lowerLimitCoder >= secondQuater)
                {
                    if (upperLimitCoder < secondQuater)
                    {
                        lowerLimitCoder = lowerLimitCoder * 2;
                        upperLimitCoder = (upperLimitCoder * 2) + 1;
                        writer.writeBit('0');

                        for (int i = 0; i < E3_counter; i++)
                            writer.writeBit('1');

                        E3_counter = 0;
                    }

                    if (lowerLimitCoder >= secondQuater)
                    {
                        lowerLimitCoder = 2 * (lowerLimitCoder - secondQuater);
                        upperLimitCoder = 2 * (upperLimitCoder - secondQuater) + 1;
                        writer.writeBit('1');

                        for (int i = 0; i < E3_counter; i++)
                            writer.writeBit('0');

                        E3_counter = 0;
                    }

                }

                while (lowerLimitCoder >= firstQuater && upperLimitCoder < thirdQuater)
                {
                    lowerLimitCoder = 2 * (lowerLimitCoder - firstQuater);
                    upperLimitCoder = 2 * (upperLimitCoder - firstQuater) + 1;
                    E3_counter++;
                }
            }


            if (lowerLimitCoder < firstQuater)
            {
                writer.writeBits("01");

                for (int i = 0; i < E3_counter; i++)
                    writer.writeBit('1');
            }
            else
            {
                writer.writeBits("10");

                for (int i = 0; i < E3_counter; i++)
                    writer.writeBit('0');
            }

            writer.finishByte();
            writer.CloseFile();
            BinReader.filestream.Close();
            //WriteToFile(filenameOut, numOfBits, output, chunks, maxFrequLength);
        }

        static void Decode(string filename, string filenameOut)
        {
            BinReader reader = new BinReader(filename);
            Chunk chunk = null;
            Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
            string encodedString = "";

            long lowerLimitCoder = 0;
            long upperLimitCoder = 0;

            //št bitov kodirnika
            int numOfBits = Convert.ToInt32(reader.readBits(8), 2);
            upperLimitCoder = (long)Math.Pow(2, numOfBits - 1) - 1;

            //št bitov za zapis frekvence
            int maxFrequLength = Convert.ToInt32(reader.readBits(8), 2);

            //št znakov
            int numOfChars = Convert.ToInt32(reader.readBits(numOfBits), 2);

            //dolžina zadnjega chunka
            int lastChunkLength = Convert.ToInt32(reader.readBits(8), 2);

            //znaki z in frekvence znakov                   
            for (int i = 0; i < numOfChars; i++)
            {
                chunk = new Chunk();
                string key = reader.readBits(numOfBits);
                chunk.frequency = Convert.ToInt32(reader.readBits(maxFrequLength), 2);

                if (i == numOfChars - 1)
                {
                    key = key.Substring(key.Length - lastChunkLength);
                }

                chunks.Add(key, chunk);
            }

            int prevUpperLimit = 0;
            int counter = 0;

            foreach (KeyValuePair<string, Chunk> c in chunks)
            {
                if (counter == 0)
                {
                    c.Value.lowerLimit = 0;
                    c.Value.upperLimit = c.Value.frequency;
                }
                else
                {
                    c.Value.lowerLimit = prevUpperLimit;
                    c.Value.upperLimit = c.Value.lowerLimit + c.Value.frequency;
                }
                prevUpperLimit = c.Value.upperLimit;
                counter++;
            }

            long secondQuater = (upperLimitCoder + 1) / 2;
            long firstQuater = secondQuater / 2;
            long thirdQuater = firstQuater * 3;
            long comulativeFrequency = 0;
            comulativeFrequency = chunks.Last().Value.upperLimit;

            //kodiran izhod
            while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                encodedString += reader.readBits(numOfBits);

            encodedString = encodedString.Trim(' ');

            BinReader.filestream.Close();
            BinWriter writer = new BinWriter(filenameOut);

            string array = "";
            int index = 0;
            for (int i = 0; i < numOfBits - 1; i++)
            {
                array += encodedString[i];
                index = i;
            }

            long step = 0;
            long v = 0;
            string chunkKey = "";

            for (int j = 0; j < comulativeFrequency; j++)
            {
                step = (upperLimitCoder - lowerLimitCoder + 1) / comulativeFrequency;

                v = (Convert.ToInt64(array, 2) - lowerLimitCoder) / step;

                foreach (KeyValuePair<string, Chunk> c in chunks)
                {
                    if (c.Value.lowerLimit <= v && c.Value.upperLimit > v)
                    {
                        chunkKey = c.Key;
                        chunk = c.Value;
                        break;
                    }
                }

                writer.writeBits(chunkKey);

                upperLimitCoder = lowerLimitCoder + step * chunk.upperLimit - 1;
                lowerLimitCoder = lowerLimitCoder + step * chunk.lowerLimit;

                while (upperLimitCoder < secondQuater || lowerLimitCoder >= secondQuater)
                {
                    if (upperLimitCoder < secondQuater)
                    {
                        lowerLimitCoder = lowerLimitCoder * 2;
                        upperLimitCoder = (upperLimitCoder * 2) + 1;
                        index++;

                        if (encodedString.Length <= index)
                        {
                            encodedString += "0";
                        }

                        array = writer.addLeadingZeros(array.Substring(1) + encodedString[index], numOfBits - 1);
                    }

                    if (lowerLimitCoder >= secondQuater)
                    {
                        lowerLimitCoder = 2 * (lowerLimitCoder - secondQuater);
                        upperLimitCoder = 2 * (upperLimitCoder - secondQuater) + 1;
                        index++;

                        if (encodedString.Length <= index)
                        {
                            encodedString += "0";
                        }

                        array = writer.addLeadingZeros(Convert.ToString((2 * (Convert.ToInt64(array, 2) - secondQuater)) + Convert.ToInt64(encodedString[index].ToString(), 2), 2), numOfBits - 1);
                    }

                }

                while (lowerLimitCoder >= firstQuater && upperLimitCoder < thirdQuater)
                {
                    lowerLimitCoder = 2 * (lowerLimitCoder - firstQuater);
                    upperLimitCoder = 2 * (upperLimitCoder - firstQuater) + 1;
                    index++;

                    if (encodedString.Length <= index)
                    {
                        encodedString += "0";
                    }

                    array = writer.addLeadingZeros(Convert.ToString((2 * (Convert.ToInt64(array, 2) - firstQuater)) + Convert.ToInt64(encodedString[index].ToString(), 2), 2), numOfBits - 1);

                }
            }


            writer.finishByte();
            writer.CloseFile();
        }

        static void ReadFile(string filename, int numOfBits, out Dictionary<string, Chunk> chunks, out int maxFrequLength)
        {
            chunks = new Dictionary<string, Chunk>();
            maxFrequLength = 0;

            try
            {
                long maxFrequency = 0;
                BinReader reader = new BinReader(filename);

                while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                {
                    string chunkString = reader.readBits(numOfBits);

                    Chunk chunk;
                    chunks.TryGetValue(chunkString, out chunk);

                    if (chunk != null)
                    {
                        chunk.frequency++;
                    }
                    else
                    {
                        chunk = new Chunk();
                        chunk.frequency = 1;
                        chunks.Add(chunkString, chunk);
                    }
                }

                int prevUpperLimit = 0;
                int counter = 0;

                foreach (KeyValuePair<string, Chunk> c in chunks)
                {
                    if (counter == 0)
                    {
                        c.Value.lowerLimit = 0;
                        c.Value.upperLimit = c.Value.frequency;
                    }
                    else
                    {

                        c.Value.lowerLimit = prevUpperLimit;
                        c.Value.upperLimit = c.Value.lowerLimit + c.Value.frequency;
                    }

                    if (c.Value.frequency > maxFrequency)
                    {
                        maxFrequency = c.Value.frequency;
                    }

                    prevUpperLimit = c.Value.upperLimit;
                    counter++;
                }

                maxFrequLength = (int)Math.Floor(Math.Log2(maxFrequency)) + 1;

                BinReader.filestream.Close();
            }
            catch
            {
                Console.WriteLine("Error reading file.");
            }
        }

        static void Main(string[] args)
        {
            #region args

            int numOfBits = 32;
            string action = "encode";

            int maxFrequLength;
            string readFile = "test/lena.bmp";
            string outFile = "test/coded/lena.txt";

            Dictionary<string, Chunk> chunks;


            bool error = false;
            if (args.Length == 3)
            {
                Console.WriteLine("Preverjam parametre");

                // akcija
                if (args[0] != null) action = args[0] == "c" ? "encode" : "decode";
                else error = true;

                // preveri vhodno 1. datoteko
                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Datoteka 1 ne obstaja.");
                    error = true;
                }
                else
                {
                    readFile = args[1];
                }

                if (args[2] != null) outFile = args[2];
                else if (args[1] != null) outFile = "out" + readFile;
                else error = true;
            }
            else
            {
                error = true;
            }

            if (error)
            {
                Console.WriteLine("Nepravilna oblika ukaza.");

                Console.WriteLine("AritmeticCoding <operacija> <vhodna datoteka> <izhodna datoteka>");
                Console.WriteLine("<operacija>");
                Console.WriteLine("\tc - kompresija");
                Console.WriteLine("\td - dekompresija");
                Console.WriteLine("<vhodna datoteka> - pot do poljubne vhodne datoteke");
                Console.WriteLine("<izhodna datoteka> - pot do poljubne izhodne datoteke");

                return;
            }

            #endregion

            Stopwatch sw1 = new Stopwatch();
            sw1.Start();

            if (action == "encode")
            {
                ReadFile(readFile, numOfBits, out chunks, out maxFrequLength);
                Encode(readFile, outFile, numOfBits, chunks, maxFrequLength);
            }
            else
            {
                Decode(readFile, outFile);
            }

            sw1.Stop();

            Console.WriteLine("\n time elapsed={0}", sw1.Elapsed);

        }
    }
}
