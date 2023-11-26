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
    public class EncryptingMenu
    {
        public static void Open()
        {
            string answer = "";

            while(answer != "0")
            {
                Console.WriteLine("What do you want to do?\n" +
                    "1 - encrypt file;\n" +
                    "2 - decrypt file;" +
                    "0 - exit\n");
                Console.Write("Answer: ");
                answer = Console.ReadLine();
                
                string[] answers = { "1", "2", "0"};
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("Answer: ");
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
                Console.WriteLine("Error. File with such name does not exist. Please write a correct name of a file");
                return;
            }

            Console.Write("Please enter a password: ");
            string password = Console.ReadLine();
            password = HashingAlgo.HashText(password);

            RC5 rc = new RC5(password);

            var plaintext = File.ReadAllBytes(filename);

            var ciphertext = rc.Encrypt2(plaintext);

            Console.WriteLine("\nSaving encrypted file...\n");
            SaveFile(ciphertext);
        }

        public static void DecryptFile()
        {
            string filename = "";
            Console.Write("Please enter a name of a file to decrypt: ");
            filename = Console.ReadLine();

            if (!File.Exists(filename))
            {
                Console.WriteLine("Error. File with such name does not exist. Please write a correct name of a file");
                return;
            }

            Console.Write("Please enter a password: ");
            string password = Console.ReadLine();
            password = HashingAlgo.HashText(password);

            RC5 rc = new RC5(password);

            var cyphertext = File.ReadAllBytes(filename);

            var plaintext = rc.Decrypt2(cyphertext);

            Console.WriteLine("\nSaving decrypted file...\n");
            SaveFile(plaintext);
        }

        public static void SaveFile(byte[] bytes)
        {
            string filename = "";
            Console.Write("Please enter a name of a file: ");
            filename = Console.ReadLine();

            char[] prohibited = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            while (filename.IndexOfAny(prohibited) != -1)
            {
                Console.WriteLine("Error. File name cannot contain any of these symbols: \\, /, :, *, ?, \", <, >, |");
                Console.Write("Please enter a name of a file: ");
                filename = Console.ReadLine();
            }

            File.WriteAllBytes(filename, bytes);

            Console.WriteLine("Result has been saved to the file \"" + filename + ".txt\" successfully");
        }
    }

    public class RC5
    {
        private const int WordSize = 32;
        private const int Rounds = 20;
        private const int KeySize = 16;

        private uint[] S;
        private uint[] P;

        public RC5(byte[] key)
        {
            if (key.Length != KeySize) Initialize(key[..KeySize]);
            else Initialize(key);
        }

        public RC5(string password)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);

            if (key.Length != KeySize) Initialize(key[..KeySize]);
            else Initialize(key);
        }

        private void Initialize(byte[] key)
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
            if (data.Length % 8 != 0)
            {
                //throw new ArgumentException("Data size must be a multiple of 8 bytes.");
                data = Pad(data);
            }

            uint[] buffer = new uint[data.Length / 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);

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

            byte[] result = new byte[data.Length];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return result;
        }

        public byte[] Encrypt2(byte[] data)
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

        public byte[] Encrypt3(byte[] data)
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

        // can crash with some files
        public byte[] Decrypt(byte[] data)
        {
            if (data.Length % 8 != 0)
            {
                throw new ArgumentException("Data size must be a multiple of 8 bytes.");
            }

            uint[] buffer = new uint[data.Length / 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);

            for (int i = 0; i < buffer.Length; i += 2)
            {
                uint a = buffer[i];
                uint b = buffer[i + 1];

                for (int round = Rounds; round > 0; round--)
                {
                    b = ((b - S[2 * round + 1]) >> (int)a | (b - S[2 * round + 1]) << (int)(32 - a)) ^ a;
                    a = ((a - S[2 * round]) >> (int)b | (a - S[2 * round]) << (int)(32 - b)) ^ b;
                }

                b -= S[1];
                a -= S[0];

                buffer[i] = a;
                buffer[i + 1] = b;
            }

            buffer = Unpad(buffer);

            byte[] result = new byte[data.Length];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return result;
        }

        public byte[] Decrypt2(byte[] data)
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
}
