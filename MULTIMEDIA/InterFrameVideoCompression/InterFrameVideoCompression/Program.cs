using GleamTech.VideoUltimate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;

namespace InterFrameVideoCompression
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

    public class Dct
    {
        double[][] c;
        double[] w;
        IEnumerable<int> Rn;

        public Dct(int N)
        {
            Rn = Enumerable.Range(0, N);
            var N2 = N * 2;
            c = Rn.Select(n => Rn.Select(k => Math.Cos((Math.PI * ((2 * n + 1) * k)) / N2)).ToArray()).ToArray();
            w = Enumerable.Repeat(Math.Sqrt(2.0 / N), N).ToArray();
            w[0] = Math.Sqrt(1.0 / N);
        }

        public double[] dct(double[] y)
        {
            var t = Rn.Select(k => w[k] * Rn.Select(n => c[n][k] * y[n]).Sum()).ToArray();
            return t;
        }

        public double[] idct(double[] y)
        {
            var t = Rn.Select(k => Rn.Select(n => w[n] * c[k][n] * y[n]).Sum()).ToArray();
            return t;
        }
    }

    public class ArithmeticCoding
    {
        public class Chunk
        {
            public int frequency;
            public int lowerLimit;
            public int upperLimit;
        }

        static void Encode(List<int> data, BinWriter writer, int numOfBits, Dictionary<string, Chunk> chunks, int maxFrequLength)
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

            //BinWriter writer = new BinWriter(filenameOut);

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
            int counter = 0;
            foreach (KeyValuePair<string, Chunk> c in chunks)
            {
                int key = int.Parse(c.Key.Trim(' '));
                string val = Convert.ToString(key, 2);

                writer.writeBits(writer.addLeadingZeros(val, numOfBits));

                foreach (char f in writer.addLeadingZeros(Convert.ToString(c.Value.frequency, 2), maxFrequLength))
                    writer.writeBit(f);

                counter++;
            }

            int numOfAllWrittenBits = 0;

            foreach (int d in data)
            {
                string chunkString = d.ToString();

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
                        numOfAllWrittenBits++;

                        for (int i = 0; i < E3_counter; i++)
                        {
                            writer.writeBit('1');
                            numOfAllWrittenBits++;
                        }

                        E3_counter = 0;
                    }

                    if (lowerLimitCoder >= secondQuater)
                    {
                        lowerLimitCoder = 2 * (lowerLimitCoder - secondQuater);
                        upperLimitCoder = 2 * (upperLimitCoder - secondQuater) + 1;
                        writer.writeBit('1');
                        numOfAllWrittenBits++;

                        for (int i = 0; i < E3_counter; i++)
                        {
                            writer.writeBit('0');
                            numOfAllWrittenBits++;
                        }

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
                numOfAllWrittenBits += 2;

                for (int i = 0; i < E3_counter; i++)
                {
                    writer.writeBit('1');
                    numOfAllWrittenBits++;
                }
            }
            else
            {
                writer.writeBits("10");
                numOfAllWrittenBits += 2;

                for (int i = 0; i < E3_counter; i++)
                {
                    writer.writeBit('0');
                    numOfAllWrittenBits++;
                }
            }

            BinWriter writer2 = new BinWriter("Video/VideoEncoded2.txt");

            foreach (char c in writer2.addLeadingZeros(Convert.ToString(numOfAllWrittenBits, 2), 32).Trim(' '))
                writer2.writeBit(c);

            writer2.CloseFile();

        }

        public static void Decode(BinReader reader, int lengthOfEncodedData, out List<int> values)
        {
            //BinReader reader = new BinReader(filename);
            Chunk chunk = null;
            Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
            string encodedString = "";
            values = new List<int>();

            long lowerLimitCoder = 0;

            //št bitov kodirnika
            int numOfBits = Convert.ToInt32(reader.readBits(8), 2);
            long upperLimitCoder = (long)Math.Pow(2, numOfBits - 1) - 1;

            //št bitov za zapis frekvence
            int maxFrequLength = Convert.ToInt32(reader.readBits(8), 2);

            //št znakov
            long numOfChars = Convert.ToInt32(reader.readBits(numOfBits), 2);

            //dolžina zadnjega chunka
            int lastChunkLength = Convert.ToInt32(reader.readBits(8), 2);

            //znaki z in frekvence znakov
            for (int i = 0; i < numOfChars; i++)
            {
                string key;
                chunk = new Chunk();

                key = reader.readBits(numOfBits);

                chunk.frequency = Convert.ToInt32(reader.readBits(maxFrequLength), 2);

                if (i == numOfChars - 1)
                    key = key.Substring(key.Length - lastChunkLength);

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
            long comulativeFrequency = chunks.Last().Value.upperLimit;

            //kodiran izhod (pri I frame-u bi moglo prebrat en frame, pri B in P pa le po 256 znavov)
            //long currPos = reader.binaryReader.BaseStream.Position;
            //while (reader.binaryReader.BaseStream.Position != (currPos + lengthOfEncodedData))
            //    encodedString += reader.readBits(numOfBits);

            encodedString += reader.readBits(lengthOfEncodedData);

            encodedString = encodedString.Trim(' ');

            string array = "";
            int index = 0;
            for (int i = 0; i < numOfBits - 1; i++)
            {
                array += encodedString[i];
                index = i;

                if (i == encodedString.Length - 1) break;
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

                // Add new value
                values.Add(Convert.ToInt32(chunkKey, 2));

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
                            encodedString += "0";

                        array = reader.addLeadingZeros(array.Substring(1) + encodedString[index], numOfBits - 1);
                    }

                    if (lowerLimitCoder >= secondQuater)
                    {
                        lowerLimitCoder = 2 * (lowerLimitCoder - secondQuater);
                        upperLimitCoder = 2 * (upperLimitCoder - secondQuater) + 1;
                        index++;

                        if (encodedString.Length <= index)
                            encodedString += "0";

                        array = reader.addLeadingZeros(Convert.ToString((2 * (Convert.ToInt64(array, 2) - secondQuater)) + Convert.ToInt64(encodedString[index].ToString(), 2), 2), numOfBits - 1);
                    }

                }

                while (lowerLimitCoder >= firstQuater && upperLimitCoder < thirdQuater)
                {
                    lowerLimitCoder = 2 * (lowerLimitCoder - firstQuater);
                    upperLimitCoder = 2 * (upperLimitCoder - firstQuater) + 1;
                    index++;

                    if (encodedString.Length <= index)
                        encodedString += "0";

                    array = reader.addLeadingZeros(Convert.ToString((2 * (Convert.ToInt64(array, 2) - firstQuater)) + Convert.ToInt64(encodedString[index].ToString(), 2), 2), numOfBits - 1);

                }
            }

            //BinReader.filestream.Close();

        }

        public static void ReadData(List<int> data, int numOfBits, BinWriter writer)
        {
            Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
            int maxFrequLength = 0;

            try
            {
                long maxFrequency = 0;

                foreach (int i in data)
                {
                    string chunkString = i.ToString();

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


                Encode(data, writer, numOfBits, chunks, maxFrequLength);
            }
            catch
            {
                Console.WriteLine("Error ArithmeticCoding -> ReadData.");
            }
        }
    }



    enum FrameTypes { I, P, B }
    class Frame
    {
        public byte[] frameData { get; set; }
        public FrameTypes frameType { get; set; }
        public bool isOriginal { get; set; }
        public List<MseObject> mseObjects { get; set; }
        public List<int> decodingReadValues { get; set; }

        public Frame(byte[] frame, FrameTypes frameType)
        {
            this.frameData = frame;
            this.frameType = frameType;
        }
        public Frame() { }
    }
    class MseObject
    {
        public List<double> msePrev;
        public List<double> mseNext;
        public double averagePrevMse;
        public double averageNextMse;

        public int xOffset;
        public int yOffset;
        public FrameTypes chosenFrameType;

        public string helperFrameToDecode { get; set; }
        public bool isOriginal { get; set; }


        public MseObject(int yOffset, int xOffset)
        {
            this.yOffset = yOffset;
            this.xOffset = xOffset;
            this.msePrev = new List<double>();
            this.mseNext = new List<double>();
            this.averagePrevMse = 0;
            this.averageNextMse = 0;
        }
        public MseObject()
        {
        }
    }


    class Program
    {
        static double Mse(byte[] frame, byte[] prevframe, int length)
        {
            double mse = 0;

            // Grayscale image has same RGB values so we only check one of them
            for (int i = 0; i < length; i += 3)
                mse += Math.Pow(frame[i] - prevframe[i], 2);

            return mse;  // nisem čisto ziher, če morem dat / 3
        }

        static void CikCak(double[,] matrix, int width, int height, out List<int> cikCak)
        {
            cikCak = new List<int>();
            var moving_up = true;
            var h = 0;
            var w = 0;

            while (cikCak.Count < width * height)
            {
                while (moving_up && h > -1 && w < height)
                {
                    cikCak.Add((int)Math.Round(matrix[h, w]));
                    h = h - 1;
                    w = w + 1;
                }
                if (moving_up && (h < 0 || w > height - 1))
                {
                    h = h + 1;
                    w = w - 1;
                    moving_up = false;
                    if (w < height - 1)
                    {
                        w = w + 1;
                    }
                    else
                    {
                        h = h + 1;
                    }
                }
                while (!moving_up && w > -1 && h < width)
                {
                    cikCak.Add((int)Math.Round(matrix[h,w]));
                    h = h + 1;
                    w = w - 1;
                }
                if (!moving_up && (w < 0 || h > width - 1))
                {
                    moving_up = true;
                    h = h - 1;
                    w = w + 1;
                    if (h < width - 1)
                    {
                        h = h + 1;
                    }
                    else
                    {
                        w = w + 1;
                    }
                }
            }
        }

        static void InverseCikCak(List<int> list, int blockSize, out List<double> inverselist)
        {
            var counter = 0;
            var moving_up = true;
            var h = 0;
            var w = 0;

            int[,] matrix = new int[blockSize, blockSize];

            while (h < blockSize - 1 || w < blockSize)
            {
                while (moving_up && h > -1 && w < blockSize)
                {
                    matrix[h,w] = list[counter];
                    counter = counter + 1;
                    h = h - 1;
                    w = w + 1;
                }
                if (moving_up && (h < 0 || w > blockSize - 1))
                {
                    h = h + 1;
                    w = w - 1;
                    moving_up = false;
                    if (w < blockSize - 1)
                    {
                        w = w + 1;
                    }
                    else
                    {
                        h = h + 1;
                    }
                }
                while (!moving_up && w > -1 && h < blockSize)
                {
                    matrix[h,w] = list[counter];
                    counter = counter + 1;
                    h = h + 1;
                    w = w - 1;
                }
                if (!moving_up && (w < 0 || h > blockSize - 1))
                {
                    moving_up = true;
                    h = h - 1;
                    w = w + 1;
                    if (h < blockSize - 1)
                    {
                        h = h + 1;
                    }
                    else
                    {
                        w = w + 1;
                    }
                }
            }

            inverselist = new List<double>();
            for (int i = 0; i < blockSize; i++)
                for (int j = 0; j < blockSize; j++)
                    inverselist.Add(matrix[i, j]);
        }

        static void CompareFrames(Frame frame, Frame prevframe, Frame nextframe, int width, int height, int M, BinWriter writer)
        {
            List<List<byte>> matrixFrame = ConvertMatrix(frame.frameData, width);
            List<List<byte>> matrixPrevFrame = ConvertMatrix(prevframe.frameData, width);
            List<List<byte>> matrixNextFrame = nextframe != null ? ConvertMatrix(nextframe.frameData, width) : null;

            List<MseObject> minMseList = new List<MseObject>();
            List<double> originalFrame = new List<double>();
            List<int> wholeFrameData = new List<int>();

            // Number of steps 16x16
            int hSteps = (int)Math.Ceiling((decimal)(height / 16));
            int wSteps = (int)Math.Ceiling((decimal)(width / 16));

            // Compare pixels
            for (int j = 0; j < hSteps; j++)
            {
                for (int i = 0; i < wSteps; i++)
                {
                    List<MseObject> mseList = new List<MseObject>();

                    // First pixel in prev/next frame
                    int topComparingY = j * 16;
                    int topComparingX = i * 16;

                    // Block 48x48 -> I (move by 4 pixels each iteration)
                    for (int hI = topComparingY - 16; hI <= topComparingY + 16; hI += 4)
                    {
                        if (hI >= 0 && height > (hI + 16))
                        {
                            for (int wI = topComparingX - 16; wI <= topComparingX + 16; wI += 4)
                            {
                                // Comparing pixels exists 
                                if (wI >= 0 && width > (wI + 16))
                                {
                                    MseObject mseObject = new MseObject(hI, wI);

                                    originalFrame = new List<double>();

                                    // Block 16x16 -> P or B
                                    for (int h = topComparingY; h < topComparingY + 16; h++)
                                    {
                                        for (int w = topComparingX; w < topComparingX + 16; w++)
                                        {
                                            var pixel = matrixFrame[h][w];
                                            var pixelPrev = matrixPrevFrame[hI + (h - topComparingY)][wI + (w - topComparingX)];

                                            mseObject.msePrev.Add(Math.Pow(pixelPrev - pixel, 2));
                                            mseObject.averagePrevMse += mseObject.msePrev.Last();

                                            if (matrixNextFrame != null)
                                            {
                                                var pixelNext = matrixNextFrame[hI + (h - topComparingY)][wI + (w - topComparingX)];
                                                mseObject.mseNext.Add(Math.Pow(pixelNext - pixel, 2));
                                                mseObject.averageNextMse += mseObject.mseNext.Last();
                                            }

                                            originalFrame.Add(pixel);
                                        }
                                    }

                                    mseObject.averagePrevMse = mseObject.averagePrevMse / (16 * 16 * 3);

                                    if (matrixNextFrame != null)
                                        mseObject.averageNextMse = mseObject.averageNextMse / (16 * 16 * 3);

                                    mseList.Add(mseObject); // Store all 16x16 values

                                }
                            }
                        }
                    }

                    if (mseList.Count > 0)
                    {
                        List<int> cikcak = new List<int>();

                        // Find best 16x16 match
                        MseObject minMsePrev = mseList.Find(x => x.averagePrevMse == mseList.Min(x => x.averagePrevMse));

                        // Encode B frames
                        if (matrixNextFrame != null)
                        {
                            MseObject minMseNext = mseList.Find(x => x.averageNextMse == mseList.Min(x => x.averageNextMse));
                            bool isNext = false;

                            if (minMseNext.averageNextMse < minMsePrev.averagePrevMse)
                            {
                                minMseList.Add(minMseNext);
                                minMseNext.chosenFrameType = nextframe.frameType;
                                isNext = true;
                            }
                            else
                            {
                                minMseList.Add(minMsePrev);
                                minMsePrev.chosenFrameType = prevframe.frameType;
                            }

                            // Encode original values
                            if (minMseList.Last().averagePrevMse > 0.75 || minMseList.Last().averageNextMse > 0.75)
                            {
                                writer.writeBits("1"); // original

                                // DCT & cikcak
                                var _dct = new Dct(256);

                                var dct = _dct.dct(originalFrame.ToArray());
                                double[,] matrix = Make2DArray(dct, 16, 16);
                                CikCak(matrix, 16, 16, out cikcak);

                                //Console.WriteLine(cikcak[0] + " " + cikcak[1] + " " + cikcak[2]);
                                ArithmeticCoding.ReadData(cikcak, 32, writer);
                            }
                            else
                            {
                                writer.writeBits("0"); // NOT original

                                // DCT & cikcak
                                var _dct = new Dct(256);
                                isNext = false;
                                var dct = _dct.dct(isNext ? minMseList.Last().mseNext.ToArray() : minMseList.Last().msePrev.ToArray());
                                double[,] matrix = Make2DArray(dct, 16, 16);
                                CikCak(matrix, 16, 16, out cikcak);

                                // Write all offsets
                                foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().xOffset, 2), 8))
                                    writer.writeBit(c);

                                foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().yOffset, 2), 8))
                                    writer.writeBit(c);

                                if (isNext)
                                {
                                    if (nextframe.frameType == FrameTypes.P)
                                        writer.writeBits("10");
                                    else
                                        writer.writeBits("11");
                                }
                                else
                                {
                                    if (prevframe.frameType == FrameTypes.P)
                                        writer.writeBits("00");
                                    else
                                        writer.writeBits("01");
                                }

                                ArithmeticCoding.ReadData(cikcak, 32, writer);

                            }
                        }
                        // Encode P frames
                        else
                        {
                            if (minMsePrev.averagePrevMse > 0.75)
                            {
                                writer.writeBits("1"); // original

                                // DCT & cikcak
                                var _dct = new Dct(256);

                                var dct = _dct.dct(originalFrame.ToArray());
                                double[,] matrix = Make2DArray(dct, 16, 16);
                                CikCak(matrix, 16, 16, out cikcak);

                                ArithmeticCoding.ReadData(cikcak, 32, writer);
                            }
                            else
                            {
                                writer.writeBits("0"); // NOT original

                                minMseList.Add(minMsePrev);

                                // DCT & cikcak
                                var _dct = new Dct(256);

                                var dct = _dct.dct(minMseList.Last().msePrev.ToArray());
                                double[,] matrix = Make2DArray(dct, 16, 16);
                                CikCak(matrix, 16, 16, out cikcak);

                                // Write all offsets
                                foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().xOffset, 2), 8))
                                    writer.writeBit(c);

                                foreach (char c in writer.addLeadingZeros(Convert.ToString(minMseList.Last().yOffset, 2), 8))
                                    writer.writeBit(c);

                                ArithmeticCoding.ReadData(cikcak, 32, writer);

                            }
                        }
                    }
                }
            }
        }

        static void EncodeFrames(Dictionary<int, Frame> frames, int height, int width, int M, int N, int fps)
        {
            KeyValuePair<int, Frame> prevIFrame = new KeyValuePair<int, Frame>();
            KeyValuePair<int, Frame> prevPFrame = new KeyValuePair<int, Frame>();
            Frame nextFrame = new Frame();
            int frameCounter = 0;

            #region Zapiši glavo datoteke

            BinWriter writer = new BinWriter("Video/VideoEncoded.txt");

            Console.WriteLine("Glava: ");

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

            //Number of frames s 16 biti
            foreach (char c in writer.addLeadingZeros(Convert.ToString(frames.Count, 2), 16).Trim(' '))
                writer.writeBit(c);

            #endregion

            foreach (KeyValuePair<int, Frame> f in frames)
            {
                if (f.Value.frameType == FrameTypes.I)
                {
                    // Save last I frame
                    prevIFrame = f;

                    // Bit 00 represents I frame
                    writer.writeBits("001"); //1, ker je original

                    List<double> data = new List<double>();
                    List<int> cikcak;
                    List<int> wholeCikCak = new List<int>();

                    // Take only every third pixel value (Because RGB pixels have same value)
                    for (int i = 0; i < f.Value.frameData.Length; i += 3)
                        data.Add(f.Value.frameData[i]);

                    // DCT & cikcak
                    var _dct = new Dct(256);

                    while (data.Count > 0)
                    {
                        var dct = _dct.dct(data.ToArray());
                        double[,] matrix = Make2DArray(dct, 16, 16);
                        CikCak(matrix, 16, 16, out cikcak);

                        wholeCikCak.AddRange(cikcak);

                        data.RemoveRange(0, 256);
                    }

                    Console.WriteLine("");

                    //Arithmetic coding
                    ArithmeticCoding.ReadData(wholeCikCak, 32, writer);

                }
                else if (f.Value.frameType == FrameTypes.P)
                {
                    // Save last P frame
                    prevPFrame = f;

                    // Bits 10 represents P frame
                    writer.writeBits("10");

                    //Console.WriteLine("\t*Frame P");

                    CompareFrames(f.Value, prevIFrame.Value, null, width, height, M, writer);

                }
                else
                {
                    //foreach (int u in f.Value.frameData)
                    //    Console.Write(" " + u);

                    // Bits 11 represents B frame
                    writer.writeBits("11");

                    //Console.WriteLine("\t*Frame B");

                    // Get previous P or I frame
                    Frame prevFrame = prevIFrame.Value;

                    if (prevPFrame.Key > prevIFrame.Key)
                        prevFrame = prevPFrame.Value;

                    // Get next P or I frame
                    frameCounter = f.Key;
                    while (true)
                    {
                        if (frames[frameCounter].frameType == FrameTypes.I)
                        {
                            nextFrame = frames[frameCounter];
                            break;
                        }
                        if (frames[frameCounter].frameType == FrameTypes.P)
                        {
                            nextFrame = frames[frameCounter];
                            break;
                        }
                        frameCounter++;
                    }

                    CompareFrames(f.Value, prevFrame, nextFrame, width, height, M, writer);

                }

                Console.Clear();
                Console.WriteLine("Kodiranje: " + Math.Round((((double)f.Key / (double)frames.Count) * 100.0), 1, MidpointRounding.AwayFromZero) + "%");

            }
            Console.Clear();
            Console.WriteLine("Kodiranje: 100%");
            Console.WriteLine("Encoded file location: \"/bin/Debug/netcoreapp3.0/Video/VideoEncoded.txt\"");

            writer.finishByte();
            writer.CloseFile();

        }

        static void Decode()
        {
            BinReader reader = new BinReader("Video/VideoEncoded.txt");
            BinReader reader2 = new BinReader("Video/VideoEncoded2.txt");

            // Read head data
            int width = Convert.ToInt32(reader.readBits(16), 2);
            int height = Convert.ToInt32(reader.readBits(16), 2);
            int fps = Convert.ToInt32(reader.readBits(16), 2);
            int N = Convert.ToInt32(reader.readBits(8), 2);
            int M = Convert.ToInt32(reader.readBits(8), 2);
            int numOfFrames = Convert.ToInt32(reader.readBits(16), 2);
            int readNumOfBits = 0;

            List<List<byte>> prevIFrame = new List<List<byte>>();
            List<List<byte>> prevPFrame = new List<List<byte>>();

            Dictionary<int, Frame> frames = new Dictionary<int, Frame>();
            int counter = 0;

            List<Bitmap> bitmapArray = new List<Bitmap>();

            for (int i = 0; i < numOfFrames; i++)
            {
                Frame f = new Frame();
                f.mseObjects = new List<MseObject>();
                List<byte> frameData = new List<byte>();

                string frameType = reader.readBits(2);

                if (frameType == "00")
                    f.frameType = FrameTypes.I;
                else if (frameType == "10")
                    f.frameType = FrameTypes.P;
                else
                    f.frameType = FrameTypes.B;

                //če je I, ga zapišem celega skupaj
                if (f.frameType == FrameTypes.I)
                {
                    f.isOriginal = reader.readBits(1) == "1" ? true : false;
                    readNumOfBits = Convert.ToInt32(reader2.readBits(32), 2);

                    //Read values
                    List<int> values;
                    ArithmeticCoding.Decode(reader, readNumOfBits, out values);
                    f.decodingReadValues = values;

                    //IDCT and inverse cikcak
                    var _dct = new Dct(256);

                    while (f.decodingReadValues.Count > 0)
                    {
                        //IDCT + inverse cikcak
                        int[,] _matrix;
                        List<double> inverseCikcak = new List<double>();
                        InverseCikCak(f.decodingReadValues, 16, out inverseCikcak);
                        var idct = _dct.idct(inverseCikcak.ToArray());

                        values = new List<int>();

                        for (int j = 0; j < idct.Length; j++)
                        {
                            if (idct[j] > 255) idct[j] = 255;
                            if (idct[j] < 0) idct[j] = 0;

                            values.Add((int)Math.Round(idct[j]));
                            values.Add((int)Math.Round(idct[j]));
                            values.Add((int)Math.Round(idct[j]));
                        }

                        byte[] byteArr = values.Select(n => {
                            return Convert.ToByte(n);
                        }).ToArray();

                        frameData.AddRange(byteArr);

                        f.decodingReadValues.RemoveRange(0, 256);
                    }

                    prevIFrame = ConvertMatrix(frameData.ToArray(), width);

                }
                else
                {
                    int[,] frameMatrix = new int[height, width];
                    int row = 0;
                    int column = 0;
                    int compareRow = 0;
                    int compareColumn = -1;

                    for (int z = 0; z < (width/16 * height/16); z++)
                    {
                        compareRow = 0;
                        compareColumn = -1;
                        MseObject mseObject = new MseObject();
                        mseObject.isOriginal = reader.readBits(1) == "1" ? true : false;

                        if (f.frameType == FrameTypes.P)
                        {
                            if (mseObject.isOriginal == false)
                            {
                                mseObject.xOffset = Convert.ToInt32(reader.readBits(8), 2);
                                mseObject.yOffset = Convert.ToInt32(reader.readBits(8), 2);
                            }

                            //Read values
                            List<int> values;
                            readNumOfBits = Convert.ToInt32(reader2.readBits(32), 2);
                            ArithmeticCoding.Decode(reader, readNumOfBits, out values);
                            f.decodingReadValues = values;

                            //IDCT and inverse cikcak
                            var _dct = new Dct(256);

                            while (f.decodingReadValues.Count > 0)
                            {
                                //IDCT + inverse cikcak
                                int[,] _matrix;
                                List<double> inverseCikcak = new List<double>();
                                InverseCikCak(f.decodingReadValues, 16, out inverseCikcak);
                                var idct = _dct.idct(inverseCikcak.ToArray());

                                values = new List<int>();

                                for (int j = 0; j < idct.Length; j++)
                                    values.Add((int)Math.Round(idct[j]));

                                int currentRow = -1;
                                for (int k = 0; k < values.Count; k++)
                                {
                                    int currentColumn = k % 16;

                                    if (currentColumn == 0)
                                        currentRow++;

                                    if (mseObject.isOriginal == true)
                                    {
                                        frameMatrix[currentRow + row * 16, currentColumn + column * 16] = values[k];
                                    }
                                    else
                                    {
                                        if ((k + 1) % 16 == 0)
                                        {
                                            compareRow++;
                                            compareColumn = -1;
                                        }
                                        compareColumn++;

                                        int val = prevIFrame[mseObject.yOffset + compareRow][mseObject.xOffset + compareColumn] - (int)Math.Sqrt(values[k]);
                                        frameMatrix[currentRow + row * 16, currentColumn + column * 16] = val;
                                    }
                                }

                                if (width / 16 - 1 <= column)
                                {
                                    row++;
                                    column = 0;
                                }
                                else
                                {
                                    column++;
                                }

                                f.decodingReadValues.RemoveRange(0, 256);
                            }
                        }

                        if (f.frameType == FrameTypes.B)
                        {
                            if(mseObject.isOriginal == false)
                            {
                                mseObject.xOffset = Convert.ToInt32(reader.readBits(8), 2);
                                mseObject.yOffset = Convert.ToInt32(reader.readBits(8), 2);

                                string comparingFrameType = reader.readBits(2);

                                if (comparingFrameType == "00")
                                    mseObject.helperFrameToDecode = "prevP";
                                else if (comparingFrameType == "01")
                                    mseObject.helperFrameToDecode = "prevI";
                                else if (comparingFrameType == "10")
                                    mseObject.helperFrameToDecode = "nextP";
                                else
                                    mseObject.helperFrameToDecode = "nextI";
                            }

                            //Read values
                            List<int> values;
                            readNumOfBits = Convert.ToInt32(reader2.readBits(32), 2);
                            ArithmeticCoding.Decode(reader, readNumOfBits, out values);
                            f.decodingReadValues = values;

                            //IDCT and inverse cikcak
                            var _dct = new Dct(256);

                            while (f.decodingReadValues.Count > 0)
                            {
                                //IDCT + inverse cikcak
                                int[,] _matrix;
                                List<double> inverseCikcak = new List<double>();
                                InverseCikCak(f.decodingReadValues, 16, out inverseCikcak);
                                var idct = _dct.idct(inverseCikcak.ToArray());

                                values = new List<int>();

                                for (int j = 0; j < idct.Length; j++)
                                    values.Add((int)Math.Round(idct[j]));

                                int currentRow = -1;
                                for(int k = 0; k < values.Count; k++)
                                {
                                    int currentColumn = k % 16;

                                    if (currentColumn == 0)
                                        currentRow++;

                                    if (mseObject.isOriginal == true)
                                    {
                                        frameMatrix[currentRow + row * 16, currentColumn + column * 16] = values[k];
                                    }
                                    else
                                    {
                                        int val = 0;

                                        if ((k + 1) % 16 == 0) {
                                            compareRow++;
                                            compareColumn = -1;
                                        }
                                        compareColumn++;

                                        if (values[k] < 0)
                                        {
                                            values[k] = Math.Abs(values[k]);
                                        }

                                        if (mseObject.helperFrameToDecode == "prevP" || mseObject.helperFrameToDecode == "nextP")
                                        {
                                            val = prevPFrame[mseObject.yOffset + compareRow][mseObject.xOffset + compareColumn] - (int)Math.Sqrt(values[k]);
                                        }
                                        else if (mseObject.helperFrameToDecode == "prevI" || mseObject.helperFrameToDecode == "nextI")
                                        {
                                            val = prevIFrame[mseObject.yOffset + compareRow][mseObject.xOffset + compareColumn] - (int)Math.Sqrt(values[k]);
                                        }

                                        frameMatrix[currentRow + row * 16, currentColumn + column * 16] = val;

                                    }
                                }

                                if(width/16 - 1 <= column)
                                {
                                    row++;
                                    column = 0;
                                }
                                else
                                {
                                    column++;
                                }

                                f.decodingReadValues.RemoveRange(0, 256);
                            }
                        }
                    }

                    for (int r = 0; r < height; r++)
                    {
                        for (int c = 0; c < width; c++)
                        {
                            if (frameMatrix[r, c] > 255)
                                frameMatrix[r, c] = 255;

                            if (frameMatrix[r, c] < 0)
                                frameMatrix[r, c] = 0;

                            frameData.Add(Convert.ToByte(frameMatrix[r, c]));
                            frameData.Add(Convert.ToByte(frameMatrix[r, c]));
                            frameData.Add(Convert.ToByte(frameMatrix[r, c]));
                        }
                    }

                    if(f.frameType == FrameTypes.P)
                        prevPFrame = ConvertMatrix(frameData.ToArray(), width);
                }


                //Save Frame
                counter++;

                Bitmap bitmap = ByteArrayToBitmap(frameData.ToArray(), width, height);
                bitmap.Save("Image/originalFrame" + counter + "_decoded.jpg", ImageFormat.Jpeg);

                bitmapArray.Add(bitmap);

            }

            Console.WriteLine("Decoded frames location: \"/bin/Debug/netcoreapp3.0/Image/\"");

            //int codec = VideoWriter.Fourcc('P', 'I', 'M', '1');
            //VideoWriter videowriter = new VideoWriter("test.mp3", codec, fps, new Size(width, height), true);

            //foreach(var bitmap in bitmapArray)
            //{
            //    videowriter.Write((Mat)bitmap);
            //}
        }

        static void Main(string[] args)
        {
            int N = 9; //določanje I okvirjev
            int M = 5; //faktor stiskanja

            Console.WriteLine("Inter Frame Video Compression Algoritm:");
            Console.WriteLine("InterFrameVideoCompression <operacija> <N> <M>");
            Console.WriteLine("<operacija>");
            Console.WriteLine("\tc - kompresija");
            Console.WriteLine("\td - dekompresija");

            #region args

            string action = "c";

            bool error = false;
            if (args.Length >= 1)
            {
                Console.WriteLine("Preverjam parametre");

                // akcija
                if (args[0] != null) 
                    action = args[0] == "c" ? "encode" : "decode";
                else 
                    error = true;

                if (args.Length >= 2 && args[1] != null) 
                    N = int.Parse(args[1]);

                if (args.Length == 3 && args[2] != null)
                    M = int.Parse(args[2]);
            }
            else
            {
                error = true;
            }

            if (error)
            {
                Console.WriteLine("Nepravilna oblika ukaza.");
                Console.WriteLine("InterFrameVideoCompression <operacija> <N> <M>");
                Console.WriteLine("<operacija>");
                Console.WriteLine("\tc - kompresija");
                Console.WriteLine("\td - dekompresija");

                return;
            }

            #endregion

            Stopwatch sw1 = new Stopwatch();
            sw1.Start();

            if (action == "encode")
            {
                Dictionary<int, Frame> frames = new Dictionary<int, Frame>();
                int height, width, fps;

                #region Read frames

                using (var videoFrameReader = new VideoFrameReader(@"Video/testvideo.mp4"))
                {
                    int count = 0;
                    long maxMse = 0;

                    height = videoFrameReader.Height;
                    width = videoFrameReader.Width;
                    fps = (int)videoFrameReader.FrameRate;

                    // Calculate max mse
                    for (int i = 0; i < width; i++)
                        for (int j = 0; j < height; j++)
                            maxMse += (long)Math.Pow(255, 2);

                    // Go trough all frames
                    foreach (Bitmap frame in videoFrameReader)
                    {
                        Rectangle rect = new Rectangle(0, 0, frame.Width, frame.Height);
                        BitmapData bmpData = frame.LockBits(rect, ImageLockMode.ReadWrite, frame.PixelFormat);

                        // Get the address of the first pixel.
                        IntPtr ptr = bmpData.Scan0;

                        // All pixel values are stored in frameRGB
                        int bytes = Math.Abs(bmpData.Stride) * frame.Height;
                        byte[] frameRGB = new byte[bytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, frameRGB, 0, bytes);

                        #region Set type of frames

                        if (count == 0)
                        {
                            Frame f = new Frame(frameRGB, FrameTypes.I);
                            frames.Add(count, f);
                        }
                        else
                        {
                            if (count % N == 0 || Mse(frameRGB, frames.Last().Value.frameData, frameRGB.Length) / maxMse > 0.75)
                            {
                                Frame f = new Frame(frameRGB, FrameTypes.I);
                                frames.Add(count, f);
                            }
                            else if (count % (N / 3.0) == 0.0)
                            {
                                Frame f = new Frame(frameRGB, FrameTypes.P);
                                frames.Add(count, f);
                            }
                            else
                            {
                                Frame f = new Frame(frameRGB, FrameTypes.B);
                                frames.Add(count, f);
                            }
                        }

                        count++;

                        #endregion

                        frame.UnlockBits(bmpData);
                    }
                }

                // Save original frame[0]
                //Bitmap bitmap = ByteArrayToBitmap(frames[0].frameData, width, height);
                //bitmap.Save("Image/originalFrame1.jpg", ImageFormat.Jpeg);

                #endregion

                EncodeFrames(frames, height, width, M, N, fps);
            }
            else
            {
                Decode();
            }


            sw1.Stop();
            Console.WriteLine("\nEncoding time elapsed={0}", sw1.Elapsed);
        }



        #region Helper functions

        static Bitmap ByteArrayToBitmap(byte[] byteIn, int imwidth, int imheight)     // byteIn the input byte array. Picture size should be known
        {
            Bitmap picOut = new Bitmap(imwidth, imheight, PixelFormat.Format24bppRgb);  //define the output picture
            BitmapData bmpData = picOut.LockBits(new Rectangle(0, 0, imwidth, imheight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            Int32 psize = bmpData.Stride * imheight;
            System.Runtime.InteropServices.Marshal.Copy(byteIn, 0, ptr, psize);
            picOut.UnlockBits(bmpData);
            return picOut;      //  e finita la commedia
        }
        static byte[] BitmapToByteArray(Bitmap img)      //img is the input image. Image width and height in pixels. PixelFormat is 24 bit per pixel in this case
        {
            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);    //define and lock the area of the picture with rectangle
            int pixelbytes = Image.GetPixelFormatSize(img.PixelFormat) / 8;     //for 24 bpp the value of pixelbytes is 3, for 32 bpp is 4, for 8 bpp is 1
            IntPtr ptr = bmpData.Scan0;      //this is a memory address, where the bitmap starts
            Int32 psize = bmpData.Stride * bmpData.Height;      // picture size in bytes
            byte[] byOut = new byte[psize];     //create the output byte array, which size is obviously the same as the input one

            System.Runtime.InteropServices.Marshal.Copy(ptr, byOut, 0, psize);      //this is a very fast method, which copies the memory content to byteOut array, but implemented for 24 bpp pictures only
            img.UnlockBits(bmpData);      //release the locked memory
            return byOut;      //  e finita la commedia
        }
        static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }
        static List<List<byte>> ConvertMatrix(byte[] arr, int width)
        {
            List<List<byte>> matrix = new List<List<byte>>();
            List<byte> list = new List<byte>();

            for (int i = 0; i < arr.Length; i += 3)
            {
                list.Add(arr[i]);

                if (list.Count == width)
                {
                    matrix.Add(list);
                    list = new List<byte>();
                }
            }

            return matrix;
        }
        #endregion
    }
}
