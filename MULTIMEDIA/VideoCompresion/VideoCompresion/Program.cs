using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GleamTech.VideoUltimate;

namespace VideoCompresion
{
    public class HuffmanNode
    {
        public string sBinaryWord;
        private bool bIsLeftNode;
        private bool bIsRightNode;
        private HuffmanNode oLeft;
        private HuffmanNode oParent;
        private HuffmanNode oRight;
        private int? cValue;
        public int iWeight;

        public HuffmanNode(){}
        public HuffmanNode(int Value){ cValue = Value; }
        public HuffmanNode(HuffmanNode Left, HuffmanNode Right)
        {
            oLeft = Left;
            oLeft.oParent = this;
            oLeft.bIsLeftNode = true;

            oRight = Right;
            oRight.oParent = this;
            oRight.bIsRightNode = true;

            iWeight = (oLeft.Weight + oRight.Weight);
        }

        public string BinaryWord
        {
            get
            {
                string sReturnValue = "";

                if (String.IsNullOrEmpty(sBinaryWord) == true)
                {
                    StringBuilder oStringBuilder = new StringBuilder();
                    HuffmanNode oHuffmanNode = this;

                    while (oHuffmanNode != null)
                    {
                        if (oHuffmanNode.bIsLeftNode == true)
                            oStringBuilder.Insert(0, "0");

                        if (oHuffmanNode.bIsRightNode == true)
                            oStringBuilder.Insert(0, "1");

                        oHuffmanNode = oHuffmanNode.oParent;
                    }

                    sReturnValue = oStringBuilder.ToString();
                    sBinaryWord = sReturnValue;
                }
                else
                {
                    sReturnValue = sBinaryWord;
                }

                return sReturnValue;
            }
        }
        public HuffmanNode Left
        {
            get
            {
                return oLeft;
            }
        }
        public HuffmanNode Parent
        {
            get
            {
                return oParent;
            }
        }
        public HuffmanNode Right
        {
            get
            {
                return oRight;
            }
        }
        public int? Value
        {
            get
            {
                return cValue;
            }
        }
        public int Weight
        {
            get
            {
                return iWeight;
            }
            set
            {
                iWeight = value;
            }
        }

        public override string ToString()
        {
            StringBuilder oStringBuilder = new StringBuilder();

            if (cValue.HasValue == true)
            {
                oStringBuilder.AppendFormat("'{0}' ({1}) - {2} ({3})", cValue.Value, iWeight, BinaryWord, BinaryWord.BinaryStringToInt32());
            }
            else
            {
                if ((oLeft != null) && (oRight != null))
                {
                    if ((oLeft.Value.HasValue == true) && (oRight.Value.HasValue == true))
                        oStringBuilder.AppendFormat("{0} + {1} ({2})", oLeft.Value, oRight.Value, iWeight);
                    else
                        oStringBuilder.AppendFormat("{0}, {1} - ({2})", oLeft, oRight, iWeight);
                }
                else
                {
                    oStringBuilder.Append(iWeight);
                }
            }

            return oStringBuilder.ToString();
        }
    }

    public class HuffmanCompressor
    {
        public class Pair
        {
            public string binaryPresentation;
            public int frequency;

            public Pair(int freq, string bin)
            {
                frequency = freq;
                binaryPresentation = bin;
            }
        }

        private HuffmanNode oRootHuffmanNode;
        private List<HuffmanNode> oValueHuffmanNodes;

        private List<HuffmanNode> BuildBinaryTree(List<int> Value)
        {
            List<HuffmanNode> oHuffmanNodes = GetInitialNodeList();

            //Update node weights
            Value.ForEach(m => oHuffmanNodes[m].Weight++);
            oHuffmanNodes = oHuffmanNodes
                .Where(m => (m.Weight > 0))
                .OrderBy(m => (m.Weight))
                .ThenBy(m => (m.Value))
                .ToList();

            //Assign parent nodes
            oHuffmanNodes = UpdateNodeParents(oHuffmanNodes);

            oRootHuffmanNode = oHuffmanNodes[0];
            oHuffmanNodes.Clear();

            //Sort nodes into a tree
            SortNodes(oRootHuffmanNode, oHuffmanNodes);

            return oHuffmanNodes;
        }

        public void Compress(List<int> data, string fileName)
        {
            List<HuffmanNode> oHuffmanNodes = BuildBinaryTree(data);

            //Filter to nodes we care about
            oValueHuffmanNodes = oHuffmanNodes
                .Where(m => (m.Value.HasValue == true))
                .OrderBy(m => (m.BinaryWord))
                .ToList();

            Dictionary<int, Pair> oCharToBinaryWordDictionary = new Dictionary<int, Pair>();

            foreach (HuffmanNode oHuffmanNode in oValueHuffmanNodes)
                oCharToBinaryWordDictionary.Add(oHuffmanNode.Value.Value, new Pair(oHuffmanNode.iWeight, oHuffmanNode.BinaryWord));


            //Zapis podatkov v datoteko
            StringBuilder oStringBuilder = new StringBuilder();
            List<byte> oByteList = new List<byte>();

            BinWriter writer = new BinWriter(fileName);

            //Število simbolov v tabeli z 8 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(oCharToBinaryWordDictionary.Count, 2), 8).Trim(' '))
            {
                writer.writeBit(c);
            }

            //Huffman tabela
            foreach (KeyValuePair<int, Pair> n in oCharToBinaryWordDictionary)
            {
                writer.writeBits(writer.addLeadingZeros(Convert.ToString(n.Key, 2), 16).Trim(' '));               //številka s 16 biti
                writer.writeBits(writer.addLeadingZeros(Convert.ToString(n.Value.frequency, 2), 16).Trim(' '));   //frekvenca pojavitve s 16 biti

            }

            //Zapis podatkov
            for (int i = 0; i < data.Count; i++)
            {
                foreach (char c in oCharToBinaryWordDictionary[data[i]].binaryPresentation)
                {
                    writer.writeBit(c);
                }
            }


            writer.finishByte();
            writer.CloseFile();

        }

        public void Decompress(string FileName, string FileNameOut)
        {
            BinReader reader = new BinReader(FileName);

            List<int> data = new List<int>();

            //preberemo število simbolov v tabeli
            int numOfSymbols = Convert.ToInt32(reader.readBits(8), 2);

            //preberemo tabelo
            for (int i = 0; i < numOfSymbols; i++)
            {
                int symbol = Convert.ToInt32(reader.readBits(16), 2);   //simbol
                int freq = Convert.ToInt32(reader.readBits(16), 2);     //frekvenca

                for (int j = 0; j < freq; j++)
                    data.Add(symbol);
            }

            //zgradimo drevo
            List<HuffmanNode> oHuffmanNodes = BuildBinaryTree(data);

            oValueHuffmanNodes = oHuffmanNodes
                .Where(m => (m.Value.HasValue == true))
                .OrderBy(m => (m.BinaryWord))
                .ToList();

            Dictionary<int, Pair> oCharToBinaryWordDictionary = new Dictionary<int, Pair>();

            foreach (HuffmanNode oHuffmanNode in oValueHuffmanNodes)
                oCharToBinaryWordDictionary.Add(oHuffmanNode.Value.Value, new Pair(oHuffmanNode.iWeight, oHuffmanNode.BinaryWord));


            //Find the zero node
            HuffmanNode oZeroHuffmanNode = oRootHuffmanNode;
            while (oZeroHuffmanNode.Left != null)
                oZeroHuffmanNode = oZeroHuffmanNode.Left;

            HuffmanNode oCurrentHuffmanNode = null;
            List<int> decoded = new List<int>();

            Console.WriteLine("");

            while (reader.binaryReader.BaseStream.Position != reader.binaryReader.BaseStream.Length +1)
            {
                char bit = reader.readBit();

                if (oCurrentHuffmanNode == null)
                    oCurrentHuffmanNode = oRootHuffmanNode;

                if (bit == '0')
                    oCurrentHuffmanNode = oCurrentHuffmanNode.Left;
                else if (bit == '1')
                    oCurrentHuffmanNode = oCurrentHuffmanNode.Right;
                else
                    break;

                if (oCurrentHuffmanNode.Left == null && oCurrentHuffmanNode.Right == null)
                {
                    // No more child nodes to choose from, so this must be a value node
                    decoded.Add((oCharToBinaryWordDictionary.FirstOrDefault(x => x.Value.binaryPresentation == oCurrentHuffmanNode.sBinaryWord).Key));
                    oCurrentHuffmanNode = null;
                }

            }

        }

        private static List<HuffmanNode> GetInitialNodeList()
        {
            List<HuffmanNode> oGetInitialNodeList = new List<HuffmanNode>();

            for (int i = Char.MinValue; i < Char.MaxValue; i++)
            {
                oGetInitialNodeList.Add(new HuffmanNode((char)(i)));
            }

            return oGetInitialNodeList;
        }

        private static void SortNodes(HuffmanNode Node, List<HuffmanNode> Nodes)
        {
            if (Nodes.Contains(Node) == false)
            {
                Nodes.Add(Node);
            }

            if (Node.Left != null)
            {
                SortNodes(Node.Left, Nodes);
            }

            if (Node.Right != null)
            {
                SortNodes(Node.Right, Nodes);
            }
        }

        private static List<HuffmanNode> UpdateNodeParents(List<HuffmanNode> Nodes)
        {
            //  Assign parent nodes
            while (Nodes.Count > 1)
            {
                int iOperations = (Nodes.Count / 2);
                for (int iOperation = 0, i = 0, j = 1; iOperation < iOperations; iOperation++, i += 2, j += 2)
                {
                    if (j < Nodes.Count)
                    {
                        HuffmanNode oParentHuffmanNode = new HuffmanNode(Nodes[i], Nodes[j]);
                        Nodes.Add(oParentHuffmanNode);

                        Nodes[i] = null;
                        Nodes[j] = null;
                    }
                }

                //  Remove null nodes
                Nodes = Nodes
                    .Where(m => (m != null))
                    .OrderBy(m => (m.Weight))   //  Choose the lowest weightings first
                    .ToList();
            }

            return Nodes;
        }


    }



    public class BinWriter
    {
        private BinaryWriter binaryWriter { set; get; }
        private string bitString { set; get; }

        private dynamic _file;

        private int counter = 0;

        public BinWriter(string file)
        {
            //this.binaryWriter = new BinaryWriter(File.Open(file, FileMode.Create));
            _file = File.Open(file, FileMode.Append);

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



    static class Program
    {
        enum FrameTypes
        {
            I,
            P,
            B
        }

        class Frame
        {
            public Bitmap frame { get; set; }
            public FrameTypes frameType { get; set; }
            public Frame(Bitmap frame, FrameTypes frameType)
            {
                this.frame = frame;
                this.frameType = frameType;
            }

            public Frame()
            {
            }
        }

        class MseObject
        {
            public List<double> mse;
            public int xOffset;
            public int yOffset;
            public double averageMse;

            public List<double> mseNext;
            public double averageMseNext;
            public FrameTypes chosenFrameType;
        }

        public static int BinaryStringToInt32(this string Value)
        {
            int iBinaryStringToInt32 = 0;

            for (int i = (Value.Length - 1), j = 0; i >= 0; i--, j++)
            {
                iBinaryStringToInt32 += ((Value[j] == '0' ? 0 : 1) * (int)(Math.Pow(2, i)));
            }

            return iBinaryStringToInt32;
        }


        static void CikCak(int[,] matrix, out List<int> cikCak)
        {
            cikCak = new List<int>();
            bool row_inc = false;
            int row = 0, col = 0;

            //First half
            for (int len = 1; len <= 16; ++len)
            {
                for (int i = 0; i < len; ++i)
                {
                    //Console.Write(matrix[row, col] + " ");
                    cikCak.Add(matrix[row, col]);

                    if (i + 1 == len) break;
                    if (row_inc) { ++row; --col; }
                    else { --row; ++col; }
                }

                if (len == 16) break;
                if (row_inc) { ++row; row_inc = false; }
                else { ++col; row_inc = true; }
            }

            if (row == 0)
            {
                if (col == 16 - 1) ++row;
                else ++col;
                row_inc = true;
            }
            else
            {
                if (row == 16 - 1) ++col;
                else ++row;
                row_inc = false;
            }

            // Print the next half 
            for (int len, diag = 15; diag > 0; --diag)
            {

                if (diag > 16) len = 16;
                else len = diag;

                for (int i = 0; i < len; ++i)
                {
                    //Console.Write(matrix[row, col] + " ");
                    cikCak.Add(matrix[row, col]);

                    if (i + 1 == len) break;
                    if (row_inc) { ++row; --col; }
                    else { ++col; --row; }
                }

                if (row == 0 || col == 16 - 1)
                {
                    if (col == 16 - 1) ++row;
                    else ++col;
                    row_inc = true;
                }
                else if (col == 0 || row == 16 - 1)
                {
                    if (row == 16 - 1) ++col;
                    else ++row;
                    row_inc = false;
                }
            }
        }
        static void DCT(List<double> list, out List<int> cikCak, int x = 16, int y = 16, int M = 0)
        {
            double pi = Math.PI;
            double ci, cj, dct, sum;

            int[,] dctMatrix = new int[x, y];

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (i == 0) ci = 1 / Math.Sqrt(y);
                    else ci = Math.Sqrt(2) / Math.Sqrt(y);

                    if (j == 0) cj = 1 / Math.Sqrt(x);
                    else cj = Math.Sqrt(2) / Math.Sqrt(x);

                    sum = 0;
                    for (int k = 0; k < y; k++)
                    {
                        for (int l = 0; l < x; l++)
                        {
                            dct = list[k * y + l] * Math.Cos((2 * k + 1) * i * pi / (2 * y)) * Math.Cos((2 * l + 1) * j * pi / (2 * x));
                            sum += dct;
                        }
                    }
                    dctMatrix[i, j] = Convert.ToInt32(Math.Round(ci * cj * sum, MidpointRounding.AwayFromZero));
                }
            }

            //Console.WriteLine("\n\n");

            //for (int i = 0; i < y; i++)
            //{
            //    for (int j = 0; j < x; j++)
            //    {
            //        Console.Write(dctMatrix[i, j] + " ");
            //    }
            //    Console.WriteLine();
            //}


            CikCak(dctMatrix, out cikCak);

            //vsa števila v pozitivna
            for (int i = 0; i < cikCak.Count; i++)
            {
                if (cikCak[i] > 0) // na sodih mestih bodo pozitivne številke
                    cikCak[i] *= 2;
                else if (cikCak[i] < 0) // na lihih mestih bodo negativne številke
                    cikCak[i] = (Math.Abs(cikCak[i] * 2)) - 1;
            }

        }
        static double Mse(Bitmap frame, Frame prevFrame, int width, int height)
        {
            double mse = 0;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var gray1 = frame.GetPixel(i, j).R;
                    var gray2 = prevFrame.frame.GetPixel(i, j).R;

                    double sum = Math.Pow(gray1 - gray2, 2);
                    mse += sum;
                }
            }
            mse = mse / (/*width * height * */3);


            return mse;
        }
        static void CompareFrames(Bitmap frame, Frame prevFrame, Frame nextFrame, int M)
        {
            int hSteps = (int)Math.Ceiling((decimal)(frame.Height / 16));
            int wSteps = (int)Math.Ceiling((decimal)(frame.Width / 16));

            List<MseObject> minMseList = new List<MseObject>();
            List<double> originalP = new List<double>();
            List<int> data = new List<int>();


            for (int j = 0; j < hSteps; j++)
            {
                for (int i = 0; i < wSteps; i++)
                {

                    List<MseObject> mseList = new List<MseObject>();

                    //prvi pixel v 16x16 bloku v P
                    int topPx = i * 16;
                    int topPy = j * 16;

                    //48x48 blok v I
                    for (int hI = topPy - 16; hI <= topPy + 32; hI++)
                    {
                        if (hI >= 0 && hI < prevFrame.frame.Height - (topPy + 16))
                        {
                            for (int wI = topPx - 16; wI <= topPx + 32; wI++)
                            {
                                if (wI >= 0 && wI < prevFrame.frame.Width - (topPx + 16))
                                {
                                    MseObject mseObject = new MseObject();
                                    mseObject.xOffset = hI;
                                    mseObject.yOffset = wI;
                                    mseObject.averageMse = 0;
                                    mseObject.mse = new List<double>();

                                    if (nextFrame != null)
                                    {
                                        mseObject.averageMseNext = 0;
                                        mseObject.mseNext = new List<double>();
                                    }

                                    originalP = new List<double>();

                                    //16x16 v P
                                    for (int h = topPy; h < topPy + 16; h++)
                                    {
                                        for (int w = topPx; w < topPx + 16; w++)
                                        {
                                            var pixelI = prevFrame.frame.GetPixel(wI + w, hI + h).R;
                                            var pixelP = frame.GetPixel(w, h).R;

                                            if (nextFrame != null)
                                            {
                                                var pixelNext = nextFrame.frame.GetPixel(wI + w, hI + h).R;
                                                mseObject.mseNext.Add(Math.Pow(pixelNext - pixelP, 2));
                                                mseObject.averageMseNext += mseObject.mseNext.Last();
                                            }

                                            originalP.Add(pixelP);

                                            mseObject.mse.Add(Math.Pow(pixelI - pixelP, 2));
                                            mseObject.averageMse += mseObject.mse.Last();
                                        }
                                    }

                                    if (nextFrame != null)
                                    {
                                        mseObject.averageMseNext = mseObject.averageMseNext / (16 * 16 * 3);
                                        mseList.Add(mseObject);
                                    }

                                    mseObject.averageMse = mseObject.averageMse / (16 * 16 * 3);
                                    mseList.Add(mseObject);
                                }
                            }
                        }
                    }

                    if (mseList.Count > 0)
                    {
                        //za hranjenje rezultatov za celotni frame
                        List<int> cikcak;
                        BinWriter writer;

                        //poiščemo objekt z najmanjšo povprečno vrednostjo mse
                        MseObject minMse = mseList.Find(x => x.averageMse == mseList.Min(x => x.averageMse));

                        if (nextFrame != null) // za B okvirje
                        {
                            //poglejmo če najdemo manjšo vrednost v naslednjem okvirju
                            MseObject minMseNext = mseList.Find(x => x.averageMseNext == mseList.Min(x => x.averageMseNext));

                            bool isPrevious = true;
                            bool isOriginal = false;

                            if (minMseNext.averageMseNext < minMse.averageMse)
                            {
                                minMseList.Add(minMseNext);
                                minMseNext.chosenFrameType = nextFrame.frameType;

                                isPrevious = false;

                                //zapišemo kateri okvir imamo prejšnji ali naslednji
                                writer = new BinWriter("Huffman.txt");
                                writer.writeBits("0"); // naslednji
                                writer.CloseFile();
                            }
                            else
                            {
                                minMseList.Add(minMse);
                                minMse.chosenFrameType = prevFrame.frameType;

                                //zapišemo kateri okvir imamo prejšnji ali naslednji
                                writer = new BinWriter("Huffman.txt");
                                writer.writeBits("1"); // prejšnji
                                writer.CloseFile();
                            }

                            //če je mse in next mse večji kot 0,75 zapišemo originalne vrednosti
                            if (minMseList.Last().averageMse > 0.75 && minMseList.Last().averageMseNext > 0.75)
                                isOriginal = true;

                            //DCT nad 16x16 blokom
                            DCT(isOriginal ? originalP : (isPrevious ? minMseList.Last().mse : minMseList.Last().mseNext), out cikcak, 16, 16, M);
                            data.AddRange(cikcak);

                        }
                        else // za P okvirje
                        {
                            bool isOriginal = false;

                            if (minMse.averageMse < 0.75)                            
                                minMseList.Add(minMse);
                            else
                                isOriginal = true;          

                            //DCT nad 16x16 blokom
                            DCT(isOriginal ? originalP : minMseList.Last().mse, out cikcak, 16, 16, M);
                            data.AddRange(cikcak);
                        }

                        //zapišemo vse odmike z 16 biti
                        writer = new BinWriter("Huffman.txt");

                        //odmik x - 16 bitov
                        foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().xOffset, 2), 16).Trim(' '))
                            writer.writeBit(c);

                        //odmik y - 16 bitov
                        foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().xOffset, 2), 16).Trim(' '))
                            writer.writeBit(c);

                        writer.CloseFile();
                    }
                }
            }


            //Zdaj je celotni frame zbran v List<int> data - zakodiramo s Huffmanom
            HuffmanCompressor oHuffmanCompressor = new HuffmanCompressor();
            oHuffmanCompressor.Compress(data, "Huffman.txt");
            Console.WriteLine("Frame je zakodiran v datoteki Huffman.txt");

        }

        static void Main(string[] args)
        {

            // ============  HUFFMANOVO KODIRANJE (primer)  ================ //

            //var list = new List<int> { 23, -4, 5, 7, 2, 7, -4, -4, 4, 4, 4, 45, 23, 5, 7, 5 };

            //HuffmanCompressor oHuffmanCompressor1 = new HuffmanCompressor();
            //oHuffmanCompressor1.Compress(list, "Huffman.txt");
            //Console.WriteLine();
            //oHuffmanCompressor.Decompress("Huffman.txt", "HuffmanOut.txt");

            // =========================================================== //


            int N = 9; //določanje I okvirjev
            int M = 2; //faktor stiskanja
            string action = "encode";

            Dictionary<int, Frame> frames = new Dictionary<int, Frame>();
            KeyValuePair<int, Frame> prevIFrame = new KeyValuePair<int, Frame>();
            KeyValuePair<int, Frame> prevPFrame = new KeyValuePair<int, Frame>();
            Frame nextFrame = new Frame();

            bool foundNextFrameForCompare = false;
            int frameCounter = 0;


            Stopwatch sw1 = new Stopwatch();
            sw1.Start();

            using (var videoFrameReader = new VideoFrameReader(@"video/test.mp4"))
            {
                int count = 0;
                long maxMse = 0;

                int height = videoFrameReader.Height;
                int width = videoFrameReader.Width;
                int fps = (int)videoFrameReader.FrameRate;

                //Izračunaj maksimalno mse
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                        maxMse += (long)Math.Pow(255, 2);
                }

                #region Preberi vse frame in določi njihove tipe

                foreach (Bitmap frame in videoFrameReader)
                {

                    if (count == 0)
                    {
                        Frame f = new Frame(frame, FrameTypes.I);
                        frames.Add(count, f);
                    }
                    else
                    {
                        if (count % (N / 3) == 0)
                        {
                            Frame f = new Frame(frame, FrameTypes.P);
                            frames.Add(count, f);
                        }
                        else if (count % N == 0 || Mse(frame, frames.Last().Value, frame.Width, frame.Height) / maxMse > 0.75)
                        {
                            Frame f = new Frame(frame, FrameTypes.I);
                            frames.Add(count, f);
                            count = 0;
                        }
                        else
                        {
                            Frame f = new Frame(frame, FrameTypes.B);
                            frames.Add(count, f);
                        }
                    }
                    count++;
                }

                #endregion


                #region Zapiši glavo datoteke

                BinWriter writer = new BinWriter("Huffman.txt");

                //Ločljivost z 2 x 16 biti (width in height)
                foreach (char c in writer.addLeadingZeros(Convert.ToString(width, 2), 16).Trim(' '))
                    writer.writeBit(c);

                foreach (char c in writer.addLeadingZeros(Convert.ToString(height, 2), 16).Trim(' '))
                    writer.writeBit(c);

                //FPS s 16 biti
                foreach (char c in writer.addLeadingZeros(Convert.ToString(fps, 2), 16).Trim(' '))
                    writer.writeBit(c);

                //N z 8 biti
                foreach (char c in writer.addLeadingZeros(Convert.ToString(N, 2), 8).Trim(' '))
                    writer.writeBit(c);

                //M z 8 biti
                foreach (char c in writer.addLeadingZeros(Convert.ToString(M, 2), 8).Trim(' '))
                    writer.writeBit(c);

                writer.finishByte();
                writer.CloseFile();

                #endregion


                #region Kodiranje okvirjev

                Console.WriteLine("Coding in process, this might take a while.");

                foreach (KeyValuePair<int, Frame> f in frames)
                {
                    Stopwatch swFrameStep = new Stopwatch();
                    swFrameStep.Start();

                    if (f.Value.frameType == FrameTypes.I)
                    {
                        //shranimo zadnji tip
                        prevIFrame = f;

                        //zapišemo kateri okvir imamo
                        writer = new BinWriter("Huffman.txt");
                        writer.writeBits("0");
                        writer.CloseFile();

                        List<double> data = new List<double>();
                        List<int> cikcak;

                        for (int i = 0; i < height; i++)
                        {
                            for(int j = 0; j < width; j++)
                            {
                                data.Add(f.Value.frame.GetPixel(j, i).R);
                            }
                        }

                        DCT(data, out cikcak, 16, 16, M);

                        //Zdaj je celotni frame zbran v List<int> cikcak - zakodiramo s Huffmanom
                        HuffmanCompressor oHuffmanCompressor = new HuffmanCompressor();
                        oHuffmanCompressor.Compress(cikcak, "Huffman.txt");
                        Console.WriteLine("Frame je zakodiran v datoteki Huffman.txt");
                    }
                    else if (f.Value.frameType == FrameTypes.P)
                    {
                        //zapišemo kateri okvir imamo
                        writer = new BinWriter("Huffman.txt");
                        writer.writeBits("10");
                        writer.CloseFile();

                        //gremo skozi f.Value.frame (16x16) in računamo MSE med vsakim od teh delov ter vsakim izmed (16x16) delov okolice iz frama prevIFrame --> okolica 48x48
                        CompareFrames(f.Value.frame, prevIFrame.Value, null, M);
                        prevPFrame = f;
                    }
                    else
                    {
                        //zapišemo kateri okvir imamo
                        writer = new BinWriter("Huffman.txt");
                        writer.writeBits("11");
                        writer.CloseFile();

                        //primerjamo B z prejšnjim okvirjem P ali I in z naslednjim okvirejm P ali I
                        Frame prevFrame = prevIFrame.Value;

                        if (prevPFrame.Key > prevIFrame.Key)
                            prevFrame = prevPFrame.Value;


                        frameCounter = f.Key;

                        while (foundNextFrameForCompare == false)
                        {

                            if (frames[frameCounter].frameType == FrameTypes.I)
                            {
                                nextFrame = frames[frameCounter];
                                foundNextFrameForCompare = true;
                            }
                            if (frames[frameCounter].frameType == FrameTypes.P)
                            {
                                nextFrame = frames[frameCounter];
                                foundNextFrameForCompare = true;
                            }
                            frameCounter++;
                        }

                        //vzamemo naslednji P ali I okvir
                        CompareFrames(f.Value.frame, prevFrame, nextFrame, M);


                    }

                    swFrameStep.Stop();
                    Console.WriteLine("\nEncoding " + f.Key + " frame time elapsed={0}", swFrameStep.Elapsed);
                }

                #endregion

                sw1.Stop();

                Console.WriteLine("\nEncoding time elapsed={0}", sw1.Elapsed);


            }
        }
    }
}
