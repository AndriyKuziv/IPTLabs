using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPTLab2.Algorithms;
using IPTLab2.FileWorks;

namespace IPTLab2.Menus
{
    public static class RandGenMenu
    {
        public static readonly string configFilePath = "equation.json";
        public static void Open()
        {
            var pars = FileWorksRandGen.ReadConfig(configFilePath);

            Console.WriteLine("\n--------------------");
            Console.WriteLine("m: {0}; a: {1}; c: {2}; x0: {3}", pars.m, pars.a, pars.c, pars.x0);
            Console.WriteLine("\n--------------------");

            long count = 0;
            string answer = "y";
            while (answer == "y")
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
}
