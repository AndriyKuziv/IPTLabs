using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPTLab2.FileWorks;
using IPTLab2.Algorithms;

namespace IPTLab2.Menus
{
    public static class MDFiveMenu
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
                message = MDFiveFileWorks.ReadFile(Console.ReadLine());
            }

            if (message is null) message = "";

            string hash = MDFive.HashText(message);
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

            string hash = MDFive.HashFile(filePath);
            Console.WriteLine("\nHash: {0}\n", hash);

            return new KeyValuePair<string, string>(filePath, hash);
        }

        public static void SaveRes(string source, string hash)
        {
            Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            string answer = Console.ReadLine();
            if (answer is null) answer = "";
            while (!MDFiveFileWorks.WriteFile(answer != "" ? answer : "res", source, hash))
            {
                Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            }
        }
    }
}
