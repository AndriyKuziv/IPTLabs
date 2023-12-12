using IPTLab2.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.FileWorks
{
    public class FileWorksRandGen
    {
        public static GeneratorParams ReadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Warning! Config file was not found");
                return null;
            }

            using StreamReader r = new StreamReader(filename);
            var json = r.ReadToEnd();
            GeneratorParams? pars = JsonConvert.DeserializeObject<GeneratorParams>(json);
            if (pars is null)
            {
                Console.WriteLine("Warning! Required parameters are missing");
            }

            return pars;
        }

        public static bool WriteFile(string filename, List<long> set, long period)
        {
            char[] prohibited = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (filename.IndexOfAny(prohibited) != -1)
            {
                Console.WriteLine("Error. File name cannot contain any of these symbols: \\, /, :, *, ?, \", <, >, |");
                return false;
            }

            StreamWriter sw = new StreamWriter(filename + ".txt");
            sw.Write("Set: [ ");
            foreach (var num in set)
            {
                sw.Write(Convert.ToString(num) + " ");
            }
            sw.WriteLine("]");
            sw.WriteLine("Period: " + Convert.ToString(period));

            sw.Close();
            Console.WriteLine("Result has been written to the file \"" + filename + ".txt\" successfully");

            return true;
        }
    }
}
