using IPTLab2.Algorithms;
using IPTLab2.Data;
using IPTLab2.FileWorks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.Menus
{
    public class RCFiveCryptoMenu
    {
        private const string encryptionExtension = ".rcf";
        private const string prefix = "RC5decoded_";

        private static string configName = "encrVal.json";

        private static long maxFileSize = 5;
        private static int wordSize = 32;
        private static int rounds = 20;
        private static int keySize = 16;

        private static Stopwatch timer = new Stopwatch();

        public static void Open()
        {
            RCFiveParams encrParams = ReadConfig(configName);
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
                Console.Write("---------------\n" +
                    "1 - encrypt a file\n" +
                    "2 - decrypt a file\n" +
                    "0 - exit\n");
                answer = Console.ReadLine();

                string[] answers = { "1", "2", "0" };
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("---------------\n" +
                        "1 - encrypt a file\n" +
                        "2 - decrypt a file\n" +
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
            password = MDFive.HashText(password);

            RCFive rc = new RCFive(password, wordSize, rounds, keySize);

            timer.Start();

            var ct = rc.EncryptFile(filename);
            if (ct is null)
            {
                Console.WriteLine("File was not encrypted");
                return;
            }

            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;
            string time = timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine("Time taken for encryption: " + time);
            timer.Reset();

            Console.WriteLine($"\n| Time taken for encryption: {time} |");
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

            Console.Write("Please enter a password: ");
            string password = Console.ReadLine();
            password = MDFive.HashText(password);

            RCFive rc = new RCFive(password, wordSize, rounds, keySize);

            timer.Start();

            var pt = rc.DecryptFile(filename);
            if (pt is null)
            {
                Console.WriteLine("File was not decrypted");
                return;
            }

            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;
            string time = timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine($"\n| Time taken for decryption: {time} |");
            timer.Reset();

            Console.WriteLine("\nSaving decrypted file...\n");
            string newFilename = prefix + filename.Substring(0, filename.Length - encryptionExtension.Length);
            FileWorksCrypto.SaveFile(pt, newFilename);
        }

        public static RCFiveParams ReadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Warning! Config file was not found");
                return null;
            }

            using StreamReader r = new StreamReader(filename);
            var json = r.ReadToEnd();
            RCFiveParams? pars = JsonConvert.DeserializeObject<RCFiveParams>(json);
            if (pars is null)
            {
                Console.WriteLine("Warning! Required parameters are missing");
            }

            return pars;
        }
    }
}
