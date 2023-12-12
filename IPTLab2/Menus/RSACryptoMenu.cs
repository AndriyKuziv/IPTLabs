using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPTLab2.Algorithms;
using IPTLab2.Data;
using IPTLab2.FileWorks;

namespace IPTLab2.Menus
{
    public static class RSACryptoMenu
    {
        private const string encryptionExtension = ".rsa";
        private const string prefix = "RSAdecoded_";

        private static int maxFileSize = 5;
        private const int megaByte = 1_048_576;

        private static RSA rsaCrypto;

        private static Stopwatch timer = new Stopwatch();

        public static void Open()
        {
            string answer = "";

            if (rsaCrypto is null)
            {
                rsaCrypto = new RSA();
            }

            while (answer != "0")
            {
                Console.Write("---------------\n" +
                    "1 - encrypt a file\n" +
                    "2 - decrypt a file\n" +
                    "3 - generate new key pair\n" +
                    "4 - read existing keys from file\n" +
                    "0 - exit\n");
                answer = Console.ReadLine();

                string[] answers = { "1", "2", "3", "4", "0" };
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("---------------\n" +
                     "1 - encrypt a file\n" +
                     "2 - decrypt a file\n" +
                     "3 - generate new key pair\n" +
                     "4 - read existing keys from file\n" +
                     "0 - exit\n");
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
                    case "3":
                        GenerateKeyPair();
                        break;
                    case "4":
                        ReadKeyPair();
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

            if (rsaCrypto is null)
            {
                rsaCrypto = new RSA();
            }

            timer.Start();

            var ct = rsaCrypto.EncryptFile(filename);

            timer.Stop();

            if (ct is null)
            {
                Console.WriteLine("File was not encrypted");
                return;
            }

            TimeSpan timeTaken = timer.Elapsed;
            string time = timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine($"\n| Time taken for encryption: {time} |");
            timer.Reset();

            Console.WriteLine("\nSaving encrypted file...\n");
            string newFilename = filename + encryptionExtension;
            FileWorksCrypto.SaveFile(ct, newFilename);
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

            if (rsaCrypto is null)
            {
                rsaCrypto = new RSA();
            }

            timer.Start();

            var pt = rsaCrypto.DecryptFile(filename);

            timer.Stop();

            if (pt is null)
            {
                Console.WriteLine("\nFile was not decrypted");
                return;
            }

            TimeSpan timeTaken = timer.Elapsed;
            string time = timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine($"\n| Time taken for decryption: {time} |");
            timer.Reset();

            Console.WriteLine("\nSaving decrypted file...");
            string newFilename = prefix + filename.Substring(0, filename.Length - encryptionExtension.Length);
            FileWorksCrypto.SaveFile(pt, newFilename);
        }

        public static void GenerateKeyPair()
        {
            rsaCrypto.GenerateKeys();
        }

        public static void ReadKeyPair()
        {
            Console.Write("Please enter a name of a keys' file: ");
            string filename = Console.ReadLine();

            if (!File.Exists(filename))
            {
                Console.WriteLine("Error! File with such name does not exist. Please write a correct name of a file");
                return;
            }

            rsaCrypto.ReadKeys(filename);
        }
    }
}
