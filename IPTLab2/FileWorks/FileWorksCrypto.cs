using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.FileWorks
{
    public static class FileWorksCrypto
    {
        public static byte[] ReadFile(string filepath)
        {
            return File.ReadAllBytes(filepath);
        }

        public static void SaveFile(byte[] bytes, string filename)
        {
            File.WriteAllBytes(filename, bytes);

            Console.WriteLine("\n| Result has been saved to the file \"" + filename + "\" successfully |");
        }

        public static void SaveJsonFile(object obj, string name)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            File.WriteAllText(name + ".json", jsonString);
        }
    }
}
