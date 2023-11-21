using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2
{

    public class Parameters
    {
        public long m {  get; set; }

        public long a { get; set; }

        public long c { get; set; }

        public long x0 { get; set; }
    }

    public static class RandGenMenu
    {
        private static readonly string _configFilePath = "equation.json";
        public static void Open()
        {
            var pars = FileWorksRandGen.ReadConfig(_configFilePath);

            Console.WriteLine("\n--------------------");
            Console.WriteLine("m: {0}; a: {1}; c: {2}; x0: {3}", pars.m, pars.a, pars.c, pars.x0);
            Console.WriteLine("\n--------------------");

            long count = 0;
            string answer = "y";
            while(answer ==  "y")
            {
                Console.Write("How many numbers do you want? (Number should not exceed 1500) | ");
                answer = Console.ReadLine();
                bool isNumeric = long.TryParse(answer, out count);
                while (!isNumeric || long.Parse(answer) > 1500)
                {
                    if (!isNumeric)
                    {
                        Console.WriteLine("Please enter a number");
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid number. It should not exceed 1500");
                    }

                    Console.Write("How many numbers do you want? (Number should not exceed 1500) | ");
                    answer = Console.ReadLine();
                }

                var res = RandGen.Gener(pars, count);
                SaveRes(res);

                Console.Write("Would you like to generate another set? (y/n) | ");
                answer = Console.ReadLine();

                while (answer != "y" && answer != "n")
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("Would you like to generate another set? (y/n) | ");
                    answer = Console.ReadLine();
                }
            }
        }

        static void SaveRes(KeyValuePair<List<long>, long> res)
        {
            Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            string answer = Console.ReadLine();
            if (answer is null) answer = "";
            while (!FileWorksRandGen.WriteFile(answer != "" ? answer : "res", res.Key, res.Value))
            {
                Console.Write("Please enter a name of a .txt file in which you want to save the results (\"res\" will be set if empty): ");
            }
        }
    }

    public static class RandGen
    {
        private static long nextXn(long a, long xn, long c, long m)
        {
            return (a * xn + c) % m;
        }

        public static KeyValuePair<List<long>, long> Gener(Parameters pars, long numCount)
        {
            long period = -1;
            long x = pars.x0;

            List<long> res = new List<long> { x };
            for(int i = 0; i < numCount - 1; i++)
            {
                x = nextXn(pars.a, x, pars.c, pars.m);
                if (period < 0 && x == pars.x0)
                {
                    period = i + 1;
                }
                res.Add(x);
            }

            if (period < 0)
            {
                long i = res.Count;
                while (period < 0)
                {
                    x = nextXn(pars.a, x, pars.c, pars.m);
                    if (x == pars.x0)
                    {
                        period = i;
                    }
                    i += 1;
                }
            }

            return new KeyValuePair<List<long>, long>(res, period);
        }
    }

    public static class FileWorksRandGen
    {
        public static Parameters ReadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Warning! Config file was not found");
                return null;
            }

            using StreamReader r = new StreamReader(filename);
            var json = r.ReadToEnd();
            Parameters? pars = JsonConvert.DeserializeObject<Parameters>(json);
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
            sw.Write("Set: [");
            foreach(var num in set)
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
