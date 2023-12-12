using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPTLab2.FileWorks
{
    public static class MDFiveFileWorks
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
}
