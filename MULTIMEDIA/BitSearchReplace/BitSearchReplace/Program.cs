using System;
using System.Collections.Generic;
using System.IO;

namespace BitSearch
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

        public BinReader(string file)
        {
            this.binaryReader = new BinaryReader(File.Open(file, FileMode.Open));
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

        static bool findReplace(string filename, string search, string replace, ref List<int> positions, string filenameOut = null, string option = "f")
        {
            Console.WriteLine(option == "f" ? "Iskani niz bitov: " + search : "Zamenjujem: '" + search + "' z nizom: '" + replace + "'");
            Console.WriteLine();

            try
            {
                BinReader reader = new BinReader(filename);
                BinWriter writer = null;

                if (option == "fr" && filenameOut != null) // find and replace
                    writer = new BinWriter(filenameOut);

                int filePos = 0;
                int searchPos = 0;
                string substring = "";

                while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length)
                {
                    string bit = reader.readBits(1);

                    substring += bit;
                    searchPos++;

                    if (search[searchPos - 1] != bit[0])
                    {
                        bool matchFound = false;

                        // odstranjujemo bite spredaj preverjamo ujemanje
                        while (searchPos > 0 && !matchFound)
                        {
                            // za vsako ko ugotovimo da ni ujemanja, zapišemo prvi bit, ki ga nato odstranimo
                            if (option == "fr")
                                writer.writeBit(substring[0]);

                            substring = substring.Remove(0, 1);
                            searchPos--;

                            matchFound = true;

                            for (int i = 0; i < substring.Length; i++)
                                if (substring[i] != search[i])
                                    matchFound = false;

                        }
                    }

                    // ujemanje je najdeno
                    if (searchPos == search.Length)
                    {
                        positions.Add(filePos - searchPos + 1);

                        searchPos = 0;
                        substring = "";

                        //zapišemo zamenjano vrednost
                        if (option == "fr")
                            foreach (char c in replace)
                                writer.writeBit(c);
                    }
                    filePos++;

                }

                // zapiši bite ki so ostali v substring
                if (option == "fr")
                {
                    foreach (char c in substring)
                        writer.writeBit(c);

                    writer.finishByte();
                    writer.CloseFile();
                }

                return true;
            }
            catch
            {
                Console.WriteLine("Prišlo je do napake.");
                return false;
            }
        }

        static void Main(string[] args)
        {
            #region agrs

            string option = "fr";
            string search = "010010";
            string replace = "1111";

            string filename = "text.txt";
            string filenameOut = "out.txt";

            List<int> positions = new List<int>();

            bool error = false;
            if (args.Length >= 3)
            {
                Console.WriteLine("Preverjam parametre");

                // preveri vhodno datoteko
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("Datoteka ne obstaja.");
                    error = true;
                }
                else
                {
                    filename = args[0];
                    filenameOut = "out_" + filename;
                }

                if (args[1] == "f" || args[1] == "fr") option = args[1];
                else error = true;

                if (args[2] != null) search = args[2];
                else error = true;

                if (option == "fr" && args[3] != null) replace = args[3];
                else if (option == "fr" && args[3] == null) error = true;

            }
            else
            {
                error = true;
            }

            Console.Clear();

            if (error)
            {

                Console.WriteLine("Nepravilna oblika ukaza.");

                Console.WriteLine("BitSearchReplace <vhodna datoteka> <opcija> <podatek1> <podatek2>");
                Console.WriteLine("<vhodna datoteka> - pot do poljubne datoteke");
                Console.WriteLine("<opcija>");
                Console.WriteLine("\tf - operacija iskanja bitov iz <podatek1>");
                Console.WriteLine("\tfr - operacija zamenjava bitov iz <podatek1> z biti iz <podatek2>");
                Console.WriteLine("<podatek1> in <podatek2> - zaporedje bitov");
                
                return;
            }

            #endregion

            findReplace(filename, search, replace, ref positions, filenameOut, option);

            if (option == "f")
                foreach (int pos in positions)
                    Console.WriteLine("Iskani niz najden na: " + pos);
            else if (option == "fr")
                Console.WriteLine("Najdenih in zamenjanih je bilo " + positions.Count + " nizov.");
        }

    }

}
