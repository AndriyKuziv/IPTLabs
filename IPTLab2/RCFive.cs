using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IPTLab2
{
    public class EncrParams
    {
        public int wordSize { get; set; }
        public int rounds { get; set; }
        public int keySize { get; set; }

        public long MaxFileSize { get; set; }
    }

    public class EncryptingMenu
    {
        private const string encryptionExtension = ".enc";

        private static string configName = "encrVal.json";

        private static long maxFileSize = 5;
        private static int wordSize = 32;
        private static int rounds = 20;
        private static int keySize = 16;

        public static void Open()
        {
            EncrParams encrParams = FileWorksRC.ReadConfig(configName);
            if (encrParams is null)
            {
                Console.WriteLine("Default parameters will be used instead");
            }
            else
            {
                maxFileSize = encrParams.MaxFileSize;
                wordSize = encrParams.wordSize;
                rounds = encrParams.rounds;
                keySize = encrParams.keySize;
            }

            string answer = "";

            while (answer != "0")
            {
                Console.Write("\nDo you want to ecnrypt a file(1), decrypt(2) or exit(0)? | ");
                answer = Console.ReadLine();

                string[] answers = { "1", "2", "0" };
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("\nDo you want to ecnrypt a file(1), decrypt(2) or exit(0)? | ");
                    answer = Console.ReadLine();
                }

                switch (answer)
                {
                    case "1":
                        EncryptFile();
                        break;
                    case "2":
                        DecryptFile();
                        break;
                }
            }
        }

        public static void EncryptFile()
        {
            var genPars = FileWorksRandGen.ReadConfig(RandGenMenu.configFilePath);
            if (genPars is null)
            {
                Console.WriteLine("Error! Encryption is not possible. Config file for generator does not exist");
                return;
            }

            string filename = "";
            Console.Write("Please enter a name of a file to encrypt: ");
            filename = Console.ReadLine();

            if (!File.Exists(filename))
            {
                Console.WriteLine("Error! File with such name does not exist. Please write a correct name of a file");
                return;
            }

            if ((new System.IO.FileInfo(filename)).Length > maxFileSize * 1_048_576)
            {
                Console.WriteLine("Error! File exceeds the size of {0}MB. Operation cannot be executed", maxFileSize);
                return;
            }

            Console.Write("Please enter a password: ");
            string password = Console.ReadLine();
            password = HashingAlgo.HashText(password);

            RC5 rc = new RC5(password, wordSize, rounds, keySize);
            var ct = rc.EncryptFile(filename);

            Console.WriteLine("\nSaving encrypted file...\n");
            string newFilename = filename + encryptionExtension;
            FileWorksRC.SaveFile(ct, newFilename);
        }

        public static void DecryptFile()
        {
            string filename = "";
            Console.Write("Please enter a name of a file to decrypt: ");
            filename = Console.ReadLine();

            if (!File.Exists(filename))
            {
                Console.WriteLine("Error! File with such name does not exist. Please write a correct name of a file");
                return;
            }

            if ((new System.IO.FileInfo(filename)).Length > maxFileSize * 1_048_576)
            {
                Console.WriteLine("Error! File exceeds the size of {0}MB. Operation cannot be executed", maxFileSize);
                return;
            }

            Console.Write("Please enter a password: ");
            string password = Console.ReadLine();
            password = HashingAlgo.HashText(password);

            RC5 rc = new RC5(password, wordSize, rounds, keySize);
            var pt = rc.DecryptFile(filename);

            Console.WriteLine("\nSaving decrypted file...\n");
            string newFilename = "decoded_" + filename.Substring(0, filename.Length - encryptionExtension.Length);
            FileWorksRC.SaveFile(pt, newFilename);
        }

    }

    public class RC5
    {
        private readonly int wordSize = 32;
        private readonly int rounds = 20;
        private readonly int keySize = 16;

        private readonly int blockSize = 8;

        private uint[] S;

        public RC5(byte[] key)
        {
            if (key.Length != keySize) Setup(key[..keySize]);
            else Setup(key);
        }

        public RC5(string password)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != keySize) Setup(key[..keySize]);
            else Setup(key);
        }

        public RC5(string password, int wordSize, int rounds, int keySize)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != keySize) key = key[..keySize];
            else Setup(key);

            Setup(key);

            this.wordSize = wordSize;
            this.rounds = rounds;
            this.keySize = keySize;
            this.blockSize = wordSize / 8 * 2;
        }

        private void Setup(byte[] key)
        {
            int c = key.Length / (wordSize / 8);
            int t = 2 * (rounds + 1);

            S = new uint[t];

            // Initialize S with a magic constant Pw and Qw
            uint Pw = 0xb7e15163;
            uint Qw = 0x9e3779b9;

            S[0] = Pw;
            for (int kk = 1; kk < t; kk++)
            {
                S[kk] = S[kk - 1] + Qw;
            }

            // Key Expansion
            int iA = 0;
            int iB = 0;
            uint[] L = new uint[c * 3];
            for (int k = 0; k < c * 3; k++)
            {
                uint A = BitConverter.ToUInt32(key, iA);
                uint B = BitConverter.ToUInt32(key, iB);

                L[k] = A + B;
                iA = (iA + 4) % key.Length;
                iB = (iB + 4) % key.Length;
            }

            int i = 0, j = 0;
            for (int k = 0; k < 3 * Math.Max(t, c); k++)
            {
                S[i] = ROTL((S[i] + L[j]), 3);
                i = (i + 1) % t;
                j = (j + 1) % c;
            }
        }

        private uint ROTL(uint val, int shift)
        {
            return (val << shift) | (val >> (wordSize - shift));
        }

        private uint ROTR(uint val, int shift)
        {
            return (val >> shift) | (val << (wordSize - shift));
        }

        public byte[] EncryptFile(string filename)
        {
            var pt = FileWorksRC.ReadFile(filename);

            int paddedLength = (pt.Length % blockSize == 0) ? pt.Length : 
                pt.Length + (blockSize - (pt.Length % blockSize));
            
            byte[] paddedData = new byte[paddedLength];
            Array.Copy(pt, paddedData, pt.Length);

            var genPars = FileWorksRandGen.ReadConfig(RandGenMenu.configFilePath);
            byte[] iv = GetIV(genPars, blockSize);

            byte[] encryptedData = new byte[paddedLength + blockSize];
            iv.CopyTo(encryptedData, 0);

            byte[] prevBlock = new byte[blockSize];
            iv.CopyTo(prevBlock, 0);

            for (int i = 0; i < paddedLength; i += blockSize)
            {
                byte[] currBlock = new byte[blockSize];
                Array.Copy(paddedData, i, currBlock, 0, blockSize);

                for (int j = 0; j < 8; j++)
                {
                    currBlock[j] ^= prevBlock[j];
                }

                byte[] encryptedBlock = Encrypt(currBlock);
                encryptedBlock.CopyTo(prevBlock, 0);

                //Array.Copy(encryptedBlock, 0, encryptedData, i, blockSize);
                encryptedBlock.CopyTo(encryptedData, i + blockSize);
            }

            return encryptedData;
        }
        private byte[] Encrypt(byte[] block)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);

            A += S[0];
            B += S[1];

            for (int i = 1; i <= rounds; i++)
            {
                A = ROTL(A ^ B, (int)B) + S[2 * i];
                B = ROTL(B ^ A, (int)A) + S[2 * i + 1];
            }

            byte[] encryptedBlock = new byte[blockSize];
            BitConverter.GetBytes(A).CopyTo(encryptedBlock, 0);
            BitConverter.GetBytes(B).CopyTo(encryptedBlock, 4);

            return encryptedBlock;
        }

        public byte[] DecryptFile(string filename)
        {
            var ct = FileWorksRC.ReadFile(filename);

            byte[] iv = new byte[blockSize];
            Array.Copy(ct, 0, iv, 0, blockSize);

            byte[] decryptedData = new byte[ct.Length - blockSize];
            byte[] prevBlock = iv;

            for (int i = blockSize; i < ct.Length; i += blockSize)
            {
                byte[] currBlock = new byte[blockSize];
                Array.Copy(ct, i, currBlock, 0, blockSize);

                byte[] decryptedBlock = Decrypt(currBlock);

                for (int j = 0; j < blockSize; j++)
                {
                    decryptedBlock[j] ^= prevBlock[j];
                }

                currBlock.CopyTo(prevBlock, 0);

                decryptedBlock.CopyTo(decryptedData, i - blockSize);
            }

            return decryptedData;
        }
        private byte[] Decrypt(byte[] block)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);

            for (int i = rounds; i > 0; i--)
            {
                B = ROTR(B - S[2 * i + 1], (int)A) ^ A;
                A = ROTR(A - S[2 * i], (int)B) ^ B;
            }

            B -= S[1];
            A -= S[0];

            byte[] decryptedBlock = new byte[blockSize];
            BitConverter.GetBytes(A).CopyTo(decryptedBlock, 0);
            BitConverter.GetBytes(B).CopyTo(decryptedBlock, 4);

            return decryptedBlock;
        }

        private byte[] GetIV(Parameters pars, long length)
        {
            return RandGen.GenerBytes(pars, length);
        }
    }

    public static class FileWorksRC
    {
        public static EncrParams ReadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Warning! Config file was not found");
                return null;
            }

            using StreamReader r = new StreamReader(filename);
            var json = r.ReadToEnd();
            EncrParams? pars = JsonConvert.DeserializeObject<EncrParams>(json);
            if (pars is null)
            {
                Console.WriteLine("Warning! Required parameters are missing");
            }

            return pars;
        }

        public static byte[] ReadFile(string filepath)
        {
            return File.ReadAllBytes(filepath);
        }

        public static void SaveFile(byte[] bytes, string filename)
        {
            File.WriteAllBytes(filename, bytes);

            Console.WriteLine("Result has been saved to the file \"" + filename + "\" successfully");
        }
    }
}
