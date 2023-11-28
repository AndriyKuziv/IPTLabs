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

namespace IPTLab2
{
    public class EncrParams
    {
        public int WordSize { get; set; }
        public int Rounds { get; set; }
        public int KeySize { get; set; }

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
                wordSize = encrParams.WordSize;
                rounds = encrParams.Rounds;
                keySize = encrParams.KeySize;
            }

            string answer = "";

            while(answer != "0")
            {
                Console.Write("\nDo you want to ecnrypt a file(1), decrypt(2) or exit(0)? | ");
                answer = Console.ReadLine();
                
                string[] answers = { "1", "2", "0"};
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("\nDo you want to ecnrypt a file(1), decrypt(2) or exit(0)? | ");
                    answer = Console.ReadLine();
                }

                switch(answer)
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

            var plaintext = FileWorksRC.ReadFile(filename);

            var ciphertext = rc.Encrypt(plaintext);

            Console.WriteLine("\nSaving encrypted file...\n");
            string newFilename = filename + encryptionExtension;
            FileWorksRC.SaveFile(ciphertext, newFilename);
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

            var cyphertext = FileWorksRC.ReadFile(filename);

            var plaintext = rc.Decrypt(cyphertext);

            Console.WriteLine("\nSaving decrypted file...\n");
            string newFilename = "decoded_" + filename.Substring(0, filename.Length - encryptionExtension.Length);
            FileWorksRC.SaveFile(plaintext, newFilename);
        }

    }

    public class RC5
    {
        private readonly int WordSize = 32;
        private readonly int Rounds = 20;
        private readonly int KeySize = 16;

        private uint[] S;
        private uint[] P;

        public RC5(byte[] key)
        {
            if (key.Length != KeySize) Setup(key[..KeySize]);
            else Setup(key);
        }

        public RC5(string password)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != KeySize) Setup(key[..KeySize]);
            else Setup(key);
        }

        public RC5(string password, int wordSize, int rounds, int keySize)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != KeySize) key = key[..KeySize];
            else Setup(key);

            Setup(key);

            WordSize = wordSize;
            Rounds = rounds;
            KeySize = keySize;
        }

        private void Setup(byte[] key)
        {
            int c = key.Length / 4;
            int t = 2 * (Rounds + 1);
            S = new uint[t];
            P = new uint[t];

            for (int i = 0; i < t; i++)
            {
                P[i] = 0;
                S[i] = 0;
            }

            for (int i = 0, j = 0; i < t; i++)
            {
                uint temp = (P[i % c] << WordSize) | P[i % c] >> (32 - WordSize);
                P[i] = temp + (uint)i;
            }

            int a = 0, b = 0;
            uint x = 0, y = 0;

            for (int k = 0, i = 0, j = 0; k < 3 * Math.Max(t, c); k++)
            {
                x = P[i] = (P[i] + x + y) << 3;
                y = S[j] = (S[j] + x + y) << (int)(x + y);
                i = (i + 1) % t;
                j = (j + 1) % c;
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            int padding = (KeySize * 2) - (data.Length % (KeySize * 2));
            byte[] paddedData = new byte[data.Length + padding];
            Buffer.BlockCopy(data, 0, paddedData, 0, data.Length);

            paddedData[^1] = (byte)padding;

            uint[] buffer = new uint[paddedData.Length / 4];
            Buffer.BlockCopy(paddedData, 0, buffer, 0, paddedData.Length);

            for (int i = 0; i < buffer.Length; i += 2)
            {
                uint a = buffer[i];
                uint b = buffer[i + 1];
                a += S[0];
                b += S[1];

                for (int round = 1; round <= Rounds; round++)
                {
                    a = ((a ^ b) << (int)b | (a ^ b) >> (int)(32 - b)) + S[2 * round];
                    b = ((b ^ a) << (int)a | (b ^ a) >> (int)(32 - a)) + S[2 * round + 1];
                }

                buffer[i] = a;
                buffer[i + 1] = b;
            }

            byte[] result = new byte[buffer.Length * 4];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return result;
        }

        public byte[] Decrypt(byte[] data)
        {
            // Perform decryption on the data
            uint[] buffer = new uint[data.Length / 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);

            for (int i = 0; i < buffer.Length; i += 2)
            {
                uint a = buffer[i];
                uint b = buffer[i + 1];

                for (int round = Rounds; round > 0; round--)
                {
                    b = ((b - S[2 * round + 1]) >> (int)a | (b - S[2 * round + 1]) << (32 - (int)a)) ^ a;
                    a = ((a - S[2 * round]) >> (int)b | (a - S[2 * round]) << (32 - (int)b)) ^ b;
                }

                b -= S[1];
                a -= S[0];

                buffer[i] = a;
                buffer[i + 1] = b;
            }

            // Retrieve the actual padding from the last byte of the decrypted data
            int padding = (int)(buffer[buffer.Length - 1] & 0xFF);
            byte[] result = new byte[data.Length - padding];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return result;
        }


        public uint[] Unpad(uint[] bytes)
        {
            int padLength = (int)(bytes[^1] & 0xFF);

            return bytes[..^padLength];
        }

        public byte[] Pad(byte[] bytes)
        {
            int padLength = ((int)KeySize * 2) - (bytes.Length % ((int)KeySize * 2));

            byte[] newArr = new byte[bytes.Length + padLength];
            for (int i = 0; i < bytes.Length; i++)
            {
                newArr[i] = bytes[i];
            }
            return newArr;
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
