using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IPTLab2
{
    public static class HashingMenu
    {
        private const int MaxFileSize = 5_242_880;
        public static void Open()
        {
            string answer = "y";
            while (answer == "y")
            {
                Console.Write("Do you wish to hash a message (m) or a file (f)? | ");
                answer = Console.ReadLine();

                while (answer != "m" && answer != "f")
                {
                    Console.WriteLine("Please enter \"f\" or \"m\"");
                    Console.Write("Do you wish to hash a message (m) or a file (f)? | ");
                    answer = Console.ReadLine();
                }

                KeyValuePair<string, string> result;

                if (answer == "m") result = HashMessage();
                else result = HashFile();

                Console.Write("Would you like to save the result? (y/n) | ");
                answer = Console.ReadLine();

                while (answer != "y" && answer != "n")
                {
                    Console.WriteLine("Please enter \"y\" or \"n\"");
                    Console.Write("Would you like to save the result? (y/n) | ");
                    answer = Console.ReadLine();
                }

                if (answer == "y") SaveRes(result.Key, result.Value);

                Console.Write("\nWould you like to generate hash again? (y/n) | ");
                answer = Console.ReadLine();

                while (answer != "y" && answer != "n")
                {
                    Console.WriteLine("Please enter \"y\" or \"n\"");
                    Console.Write("Would you like to generate hash again? (y/n) | ");
                    answer = Console.ReadLine();
                }
            }
        }

        public static KeyValuePair<string, string> HashMessage()
        {
            string answer = "";

            Console.Write("Would you like to hash a message from a file or enter it by yourself?(f/e) | ");
            answer = Console.ReadLine();

            while (answer != "f" && answer != "e")
            {
                Console.WriteLine("Please enter \"f\" or \"e\"");
                Console.Write("Would you like to hash a message from a file or enter it by yourself?(f/e) | ");
                answer = Console.ReadLine();
            }

            string message;

            if (answer == "e")
            {
                Console.Write("Please enter a message: ");
                message = Console.ReadLine();
            }
            else
            {
                Console.Write("Please enter a name of a .txt file to read from: ");
                message = FileWorks.ReadFile(Console.ReadLine());
            }

            if (message is null) message = "";

            string hash = HashingAlgo.HashText(message);
            Console.WriteLine("\nHash: {0}\n", hash);


            return new KeyValuePair<string, string>(message, hash);
        }

        public static KeyValuePair<string, string> HashFile()
        {
            Console.Write("Please enter a name of a file to hash: ");
            string filePath = Console.ReadLine();
            bool isOk = !string.IsNullOrEmpty(filePath) && File.Exists(filePath) 
                && (new System.IO.FileInfo(filePath).Length <= 5_242_880);
            while (!isOk)
            {
                if (string.IsNullOrEmpty(filePath)) Console.WriteLine("File name cannot be empty");
                else if (!File.Exists(filePath)) Console.WriteLine("File with such name does not exist");
                else Console.WriteLine("File size must not exceed 5 MB");
                
                Console.Write("Please enter a name of a file to hash: ");
                filePath = Console.ReadLine();

                isOk = !string.IsNullOrEmpty(filePath) && File.Exists(filePath)
                    && (new System.IO.FileInfo(filePath).Length <= MaxFileSize);
            }

            string hash = HashingAlgo.HashFile(filePath);
            Console.WriteLine("\nHash: {0}\n", hash);
            
            return new KeyValuePair<string, string>(filePath, hash);
        }

        public static void SaveRes(string source, string hash)
        {
            Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            string answer = Console.ReadLine();
            if (answer is null) answer = "";
            while (!FileWorks.WriteFile(answer != "" ? answer : "res", source, hash))
            {
                Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            }
        }
    }


    public static class HashingAlgo
    {
        public static string HashText(string message)
        {
            // Convert original message to big-endian bitarray
            BitArray msgLE = Utility.StrToBitArray(message);
            BitArray msgBE = Utility.ToBigEndian(msgLE);
            return HashBitArray(msgBE);
        }

        public static string HashFile(string filePath)
        {
            // Convert file to big-endian bitarray
            BitArray msgLE = Utility.FileToBitArray(filePath);
            BitArray msgBE = Utility.ToBigEndian(msgLE);
            return HashBitArray(msgBE);
        }

        // Main
        private static string HashBitArray(BitArray bitArrayBE)
        {
            int paddedLength;

            BitArray paddedMessage = ApplyPadding(bitArrayBE, out paddedLength);
            BitArray lowEndianArr = AppendLength(bitArrayBE, paddedLength, paddedMessage);
            string output = Compute(lowEndianArr);

            return output;
        }

        // Step 1
        private static BitArray ApplyPadding(BitArray message, out int paddedLength)
        {
            int len = message.Length;
            paddedLength = len + (512 - len % 512);

            paddedLength -= 64;

            if (paddedLength <= len)
            {
                paddedLength += 512;
            }

            BitArray paddedMessage = new BitArray(paddedLength);

            for(int i = 0; i < len; i++)
            {
                paddedMessage[i] = message[i];
            }

            paddedMessage[len] = true;

            return paddedMessage;
        }

        // Step 2
        private static BitArray AppendLength(BitArray originalInput, int limit, BitArray paddedMessage)
        {
            BitArray paddedMsgWLength = new BitArray(limit + 64);

            // Copy the padded message into the new array
            int paddedMsgWLengthIndex = 0;
            for (; paddedMsgWLengthIndex < paddedMessage.Length; paddedMsgWLengthIndex++)
            {
                paddedMsgWLength[paddedMsgWLengthIndex] = paddedMessage[paddedMsgWLengthIndex];
            }


            // Get 64-bit representation of length of original message
            byte[] msgLengthBytes = BitConverter.GetBytes(originalInput.Length);
            BitArray msgLengthBitsLE = new BitArray(msgLengthBytes);

            // Invert array 
            BitArray msgLengthBE = new BitArray(msgLengthBitsLE.Length);
            for (int i = 0; i < msgLengthBitsLE.Length; i++)
            {
                bool bit = msgLengthBitsLE[i];
                msgLengthBE[msgLengthBE.Length - 1 - i] = bit;
            }

            // Get low-order 32 bit word from message length
            BitArray lowOrderWord = new BitArray(32);
            for (int i = 0; i < 32; i++)
            {
                bool bitValue = false;

                if (msgLengthBE.Length - 1 - i >= 0)
                    bitValue = msgLengthBE[msgLengthBE.Length - 1 - i];


                lowOrderWord[lowOrderWord.Length - 1 - i] = bitValue;
            }

            // Get high-order 32 bit word from message length
            BitArray highOrderWord = new BitArray(32);
            for (int i = 0; i < 32; i++)
            {
                bool bitValue = false;

                if (msgLengthBE.Length - 1 - i - 32 >= 0)
                    bitValue = msgLengthBE[msgLengthBE.Length - 1 - i - 32];


                highOrderWord[lowOrderWord.Length - 1 - i] = bitValue;
            }

            // Invert words, last 8 bit in word have to be the first now and so on
            BitArray lowOrderWordInv = Utility.InvertWordBitArray(lowOrderWord);
            BitArray highOrderWordInv = Utility.InvertWordBitArray(highOrderWord);

            // Append low order word to padded message first
            for (int i = 0; paddedMsgWLengthIndex < paddedMessage.Length + 32; paddedMsgWLengthIndex++)
            {
                paddedMsgWLength[paddedMsgWLengthIndex] = lowOrderWordInv[i];
                i++;
            }

            // Append high order word to padded message second
            for (int i = 0; paddedMsgWLengthIndex < paddedMessage.Length + 64; paddedMsgWLengthIndex++)
            {
                paddedMsgWLength[paddedMsgWLengthIndex] = highOrderWordInv[i];
                i++;
            }

            // Convert to little endian
            BitArray paddedMsgWLengthLE = Utility.ToLittleEndian(paddedMsgWLength);

            return paddedMsgWLengthLE;
        }

        // Steps 3, 4, 5
        private static string Compute(BitArray message)
        {
            // Step 3

            uint A = 0x67452301;
            uint B = 0xefcdab89;
            uint C = 0x98badcfe;
            uint D = 0x10325476;

            // Step 4

            // Process 16 word blocks
            for (int i = 0; i < message.Length; i += 512)
            {
                BitArray block = new BitArray(512);
                for (int k = 0; k < 512; k++)
                {
                    block[k] = message[i + k];
                }


                uint[] X = new uint[16]; // each field is 1 byte
                byte[] blockByteArr = Utility.ConvertToByteArray(block);

                // Copy block i into X
                int xIndex = 0;
                for (int l = 0; l < blockByteArr.Length; l += 4)
                {
                    byte[] bytes = new byte[4];
                    bytes[0] = blockByteArr[l + 0];
                    bytes[1] = blockByteArr[l + 1];
                    bytes[2] = blockByteArr[l + 2];
                    bytes[3] = blockByteArr[l + 3];

                    X[xIndex] = BitConverter.ToUInt32(bytes, 0);
                    xIndex++;
                }


                // Save the original values at the beginning of this block
                uint AA = A;
                uint BB = B;
                uint CC = C;
                uint DD = D;

                // Round 1 (16 operations)
                A = Machinations.Round1Op(A, B, C, D, 0, 7, 1, X);
                D = Machinations.Round1Op(D, A, B, C, 1, 12, 2, X);
                C = Machinations.Round1Op(C, D, A, B, 2, 17, 3, X);
                B = Machinations.Round1Op(B, C, D, A, 3, 22, 4, X);

                A = Machinations.Round1Op(A, B, C, D, 4, 7, 5, X);
                D = Machinations.Round1Op(D, A, B, C, 5, 12, 6, X);
                C = Machinations.Round1Op(C, D, A, B, 6, 17, 7, X);
                B = Machinations.Round1Op(B, C, D, A, 7, 22, 8, X);

                A = Machinations.Round1Op(A, B, C, D, 8, 7, 9, X);
                D = Machinations.Round1Op(D, A, B, C, 9, 12, 10, X);
                C = Machinations.Round1Op(C, D, A, B, 10, 17, 11, X);
                B = Machinations.Round1Op(B, C, D, A, 11, 22, 12, X);

                A = Machinations.Round1Op(A, B, C, D, 12, 7, 13, X);
                D = Machinations.Round1Op(D, A, B, C, 13, 12, 14, X);
                C = Machinations.Round1Op(C, D, A, B, 14, 17, 15, X);
                B = Machinations.Round1Op(B, C, D, A, 15, 22, 16, X);


                // Round 2 (16 operations)
                A = Machinations.Round2Op(A, B, C, D, 1, 5, 17, X);
                D = Machinations.Round2Op(D, A, B, C, 6, 9, 18, X);
                C = Machinations.Round2Op(C, D, A, B, 11, 14, 19, X);
                B = Machinations.Round2Op(B, C, D, A, 0, 20, 20, X);

                A = Machinations.Round2Op(A, B, C, D, 5, 5, 21, X);
                D = Machinations.Round2Op(D, A, B, C, 10, 9, 22, X);
                C = Machinations.Round2Op(C, D, A, B, 15, 14, 23, X);
                B = Machinations.Round2Op(B, C, D, A, 4, 20, 24, X);

                A = Machinations.Round2Op(A, B, C, D, 9, 5, 25, X);
                D = Machinations.Round2Op(D, A, B, C, 14, 9, 26, X);
                C = Machinations.Round2Op(C, D, A, B, 3, 14, 27, X);
                B = Machinations.Round2Op(B, C, D, A, 8, 20, 28, X);

                A = Machinations.Round2Op(A, B, C, D, 13, 5, 29, X);
                D = Machinations.Round2Op(D, A, B, C, 2, 9, 30, X);
                C = Machinations.Round2Op(C, D, A, B, 7, 14, 31, X);
                B = Machinations.Round2Op(B, C, D, A, 12, 20, 32, X);

                // Round 3 (16 operations)
                A = Machinations.Round3Op(A, B, C, D, 5, 4, 33, X);
                D = Machinations.Round3Op(D, A, B, C, 8, 11, 34, X);
                C = Machinations.Round3Op(C, D, A, B, 11, 16, 35, X);
                B = Machinations.Round3Op(B, C, D, A, 14, 23, 36, X);

                A = Machinations.Round3Op(A, B, C, D, 1, 4, 37, X);
                D = Machinations.Round3Op(D, A, B, C, 4, 11, 38, X);
                C = Machinations.Round3Op(C, D, A, B, 7, 16, 39, X);
                B = Machinations.Round3Op(B, C, D, A, 10, 23, 40, X);

                A = Machinations.Round3Op(A, B, C, D, 13, 4, 41, X);
                D = Machinations.Round3Op(D, A, B, C, 0, 11, 42, X);
                C = Machinations.Round3Op(C, D, A, B, 3, 16, 43, X);
                B = Machinations.Round3Op(B, C, D, A, 6, 23, 44, X);

                A = Machinations.Round3Op(A, B, C, D, 9, 4, 45, X);
                D = Machinations.Round3Op(D, A, B, C, 12, 11, 46, X);
                C = Machinations.Round3Op(C, D, A, B, 15, 16, 47, X);
                B = Machinations.Round3Op(B, C, D, A, 2, 23, 48, X);


                // Round 4 (16 operations)
                A = Machinations.Round4Op(A, B, C, D, 0, 6, 49, X);
                D = Machinations.Round4Op(D, A, B, C, 7, 10, 50, X);
                C = Machinations.Round4Op(C, D, A, B, 14, 15, 51, X);
                B = Machinations.Round4Op(B, C, D, A, 5, 21, 52, X);

                A = Machinations.Round4Op(A, B, C, D, 12, 6, 53, X);
                D = Machinations.Round4Op(D, A, B, C, 3, 10, 54, X);
                C = Machinations.Round4Op(C, D, A, B, 10, 15, 55, X);
                B = Machinations.Round4Op(B, C, D, A, 1, 21, 56, X);

                A = Machinations.Round4Op(A, B, C, D, 8, 6, 57, X);
                D = Machinations.Round4Op(D, A, B, C, 15, 10, 58, X);
                C = Machinations.Round4Op(C, D, A, B, 6, 15, 59, X);
                B = Machinations.Round4Op(B, C, D, A, 13, 21, 60, X);

                A = Machinations.Round4Op(A, B, C, D, 4, 6, 61, X);
                D = Machinations.Round4Op(D, A, B, C, 11, 10, 62, X);
                C = Machinations.Round4Op(C, D, A, B, 2, 15, 63, X);
                B = Machinations.Round4Op(B, C, D, A, 9, 21, 64, X);

                // Increase each of the four registers by the value it had at the start of this block
                A = A + AA;
                B = B + BB;
                C = C + CC;
                D = D + DD;
            }


            // Step 5

            // Convert to byte array
            byte[] ABytes = BitConverter.GetBytes(A);
            byte[] BBytes = BitConverter.GetBytes(B);
            byte[] CBytes = BitConverter.GetBytes(C);
            byte[] DBytes = BitConverter.GetBytes(D);

            // Reverse bytes
            byte[] reversedBytesA = new byte[ABytes.Length];
            byte[] reversedBytesB = new byte[BBytes.Length];
            byte[] reversedBytesC = new byte[CBytes.Length];
            byte[] reversedBytesD = new byte[DBytes.Length];

            for (int k = ABytes.Length - 1, l = 0; k >= 0; k--, l++)
                reversedBytesA[l] = ABytes[k];

            for (int k = BBytes.Length - 1, l = 0; k >= 0; k--, l++)
                reversedBytesB[l] = BBytes[k];

            for (int k = CBytes.Length - 1, l = 0; k >= 0; k--, l++)
                reversedBytesC[l] = CBytes[k];

            for (int k = DBytes.Length - 1, l = 0; k >= 0; k--, l++)
                reversedBytesD[l] = DBytes[k];

            // Build hashed output to display
            var p1 = BitConverter.ToInt32(reversedBytesA, 0);
            var p2 = BitConverter.ToInt32(reversedBytesB, 0);
            var p3 = BitConverter.ToInt32(reversedBytesC, 0);
            var p4 = BitConverter.ToInt32(reversedBytesD, 0);
            var res = p1.ToString("X8") + p2.ToString("X8") + p3.ToString("X8") + p4.ToString("X8");
            res = res.ToLower();


            return res;
        }


    }

    static class Machinations
    {
        private static readonly uint[] T =
        {
            0xd76aa478,0xe8c7b756,0x242070db,0xc1bdceee,
            0xf57c0faf,0x4787c62a,0xa8304613,0xfd469501,
            0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,
            0x6b901122,0xfd987193,0xa679438e,0x49b40821,
            0xf61e2562,0xc040b340,0x265e5a51,0xe9b6c7aa,
            0xd62f105d,0x2441453,0xd8a1e681,0xe7d3fbc8,
            0x21e1cde6,0xc33707d6,0xf4d50d87,0x455a14ed,
            0xa9e3e905,0xfcefa3f8,0x676f02d9,0x8d2a4c8a,
            0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,
            0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,
            0x289b7ec6,0xeaa127fa,0xd4ef3085,0x4881d05,
            0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
            0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,
            0x655b59c3,0x8f0ccc92,0xffeff47d,0x85845dd1,
            0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,
            0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391
        };


        public static uint Round1Op(uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint[] X)
        {
            uint temp = (a + F(b, c, d) + X[k] + T[i - 1]);
            uint temp2 = ((temp >> 32 - s) | (temp << s)); // bitwise rotation
            a = b + temp2;

            return a;
        }

        public static uint Round2Op(uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint[] X)
        {
            var temp = (a + G(b, c, d) + X[k] + T[i - 1]);
            var temp2 = ((temp >> 32 - s) | (temp << s)); // bitwise rotation
            a = b + temp2;

            return a;
        }

        public static uint Round3Op(uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint[] X)
        {
            var temp = (a + H(b, c, d) + X[k] + T[i - 1]);
            var temp2 = ((temp >> 32 - s) | (temp << s)); // bitwise rotation
            a = b + temp2;

            return a;
        }

        public static uint Round4Op(uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint[] X)
        {
            var temp = (a + I(b, c, d) + X[k] + T[i - 1]);
            var temp2 = ((temp >> 32 - s) | (temp << s)); // bitwise rotation
            a = b + temp2;

            return a;
        }


        // X, Y and Z - 32 bit words, output is 32 bit word 

        private static uint F(uint X, uint Y, uint Z)
        {
            return (X & Y) | (~X & Z);
        }

        private static uint G(uint X, uint Y, uint Z)
        {
            return (X & Z) | (Y & ~Z);
        }

        private static uint H(uint X, uint Y, uint Z)
        {
            return X ^ Y ^ Z;
        }

        private static uint I(uint X, uint Y, uint Z)
        {
            return Y ^ (X | ~Z);
        }
    }

    static class Utility
    {
        public static BitArray ToLittleEndian(BitArray bigEndianArr)
        {
            BitArray littleEndianArr = new BitArray(bigEndianArr.Length);
            for (int i = 0, newArrCounter = 0; newArrCounter < bigEndianArr.Length; i += 8)
            {
                for (int k = 7; k >= 0; k--)
                    littleEndianArr[newArrCounter++] = bigEndianArr[i + k];
            }

            return littleEndianArr;
        }

        public static BitArray ToBigEndian(BitArray littleEndianArr)
        {
            BitArray bigEndianArr = new BitArray(littleEndianArr.Length);
            for (int i = 0, bitArrayBECounter = 0; bitArrayBECounter < littleEndianArr.Length; i += 8)
            {
                for (int k = 7; k >= 0; k--)
                    bigEndianArr[bitArrayBECounter++] = littleEndianArr[i + k];
            }

            return bigEndianArr;
        }


        public static BitArray InvertWordBitArray(BitArray bitArray)
        {
            // create new array to hold 32 bit ( = 4 byte )
            BitArray invertedArr = new BitArray(32);

            // iterate backwards through every byte and insert them into the new array
            int invArrCounter = 0;
            for (int l = 0; l < 4; l++)
            {
                int passedBits = l * 8;
                for (int i = 8 + passedBits; i > passedBits; i--)
                {
                    invertedArr[invArrCounter] = bitArray[bitArray.Length - i];
                    invArrCounter++;
                }

            }

            return invertedArr;
        }

        public static BitArray StrToBitArray(string input)
        {
            byte[] inputByteArr = Encoding.UTF8.GetBytes(input);

            BitArray inputBitArrayLE = new BitArray(inputByteArr);
            return inputBitArrayLE;
        }

        public static BitArray FileToBitArray(string filePath)
        {
            byte[] inputByteArr = File.ReadAllBytes(filePath);

            BitArray inputBitArrayLE = new BitArray(inputByteArr);
            return inputBitArrayLE;
        }

        public static byte[] ConvertToByteArray(BitArray bits)
        {
            byte[] bytes = new byte[bits.Length / 8];
            bits.CopyTo(bytes, 0);
            return bytes;
        }
    }


    public static class FileWorks
    {
        public static string ReadFile(string filename)
        {
            StreamReader sr = new StreamReader(filename + ".txt");
            string message = sr.ReadLine();
            if (Regex.IsMatch(message, @"\p{isCyrillic}"))
            {
                Console.WriteLine("Error. Message cannot contain cyrillic.");
                return null;
            }
            if (message is null)
            {
                message = "";
            }
            sr.Close();

            Console.WriteLine("File \"" + filename + ".txt\" has been read successfully");

            return message;
        }

        public static bool WriteFile(string filename, string source, string hash)
        {
            char[] prohibited = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (filename.IndexOfAny(prohibited) != -1)
            {
                Console.WriteLine("Error. File name cannot contain any of these symbols: \\, /, :, *, ?, \", <, >, |");
                return false;
            }

            StreamWriter sw;
            if (File.Exists(filename + ".txt"))
            {
                sw = File.AppendText(filename + ".txt");
            }
            else
            {
                sw = new StreamWriter(filename + ".txt");
            }
            sw.WriteLine("Source: " + source);
            sw.WriteLine("Generated hash: " + hash);
            sw.Close();

            Console.WriteLine("Result has been written to the file \"" + filename + ".txt\" successfully");
            return true;
        }
    }



    public class MDFive
    {
        private string message = string.Empty;
        private string hash = string.Empty;

        public void GenerateHash()
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(message);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                hash = Convert.ToHexString(hashBytes);
                Console.WriteLine("Hash has been generated successfully");
            }

        }

        public void SetMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) this.message = "";
            else this.message = message;
            hash = string.Empty;
        }

        public string GetHash()
        {
            return string.IsNullOrEmpty(hash) ? "Hash is not generated" : hash;
        }

        public string GetMessage()
        {
            return message;
        }

    }

}
