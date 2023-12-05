using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.CodeDom.Compiler;

namespace IPTLab2
{
    public class Keys
    {
        public string publicKey { get; set; }
        public string privateKey { get; set; }
    }

    public static class RSACryptoMenu
    {
        private const string encryptionExtension = ".enc";

        private static int maxFileSize = 5;
        private const int megaByte = 1_048_576;

        private static RSACrypto rsaCrypto;

        public static void Open()
        {
            string answer = "";

            if (rsaCrypto is null)
            {
                rsaCrypto = new RSACrypto();
            }

            while (answer != "0")
            {
                Console.Write("\n1 - encrypt a file\n" +
                    "2 - decrypt\n" +
                    "3 - generate new key pair\n" +
                    "4 - read existing keys from file\n" +
                    "0 - exit\n");
                answer = Console.ReadLine();

                string[] answers = { "1", "2", "3", "4", "0" };
                while (!answers.Contains(answer))
                {
                    Console.WriteLine("Please enter a valid answer");
                    Console.Write("\n1 - encrypt a file\n" +
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
                rsaCrypto = new RSACrypto();
            }
            var ct = rsaCrypto.EncryptFile(filename);
            if(ct is null)
            {
                Console.WriteLine("File was not encrypted");
                return;
            }

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
                rsaCrypto = new RSACrypto();
            }
            var pt = rsaCrypto.DecryptFile(filename);
            if (pt is null)
            {
                Console.WriteLine("File was not decrypted");
                return;
            }

            Console.WriteLine("\nSaving decrypted file...\n");
            string newFilename = "decoded_" + filename.Substring(0, filename.Length - encryptionExtension.Length);
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

    public class RSACrypto
    {
        private static int keySize = 4096;
        private static string keyPairName = "keyPair.json";

        private static string publicKey;
        private static string privateKey;

        public RSACrypto()
        {
            ReadKeys(keyPairName);
            if (publicKey is null ||  privateKey is null)
            {
                Console.WriteLine("Warning! One or more keys were not set. Please specify a file containing a key pair or " +
                    "generate a new one");
            }
        }

        public void ReadKeys(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Config file was not found");
                return;
            }

            using StreamReader r = new StreamReader(filename);
            var json = r.ReadToEnd();
            Keys? pars = JsonConvert.DeserializeObject<Keys>(json);
            if (pars is null)
            {
                Console.WriteLine("Warning! Some keys are missing");
            }
            publicKey = pars.publicKey;
            privateKey = pars.privateKey;
            
            Console.WriteLine("Public and private keys have been set");
        }

        public void GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                privateKey = rsa.ToXmlString(true);
                publicKey = rsa.ToXmlString(false);

                SaveKeys(publicKey, privateKey);
                Console.WriteLine("New public and private keys have been generated and saved");
            }
        }

        private void SaveKeys(string publicKey, string privateKey)
        {
            string name = "keyPair";
            int num = 1;
            if (File.Exists(name + ".json"))
            {
                string temp = name + Convert.ToString(num);
                while (File.Exists(temp + ".json"))
                {
                    num++;
                    temp = name + Convert.ToString(num);
                }
                name = temp;
            }

            FileWorksCrypto.SaveJsonFile(new Keys
            {
                privateKey = privateKey,
                publicKey = publicKey
            }, name);
        }

        public byte[] EncryptFile(string filename)
        {
            if(string.IsNullOrEmpty(publicKey))
            {
                Console.WriteLine("Error! Public key is not set");
                return null;
            }

            byte[] bytes = FileWorksCrypto.ReadFile(filename);

            byte[] res = Encrypt(publicKey, bytes);

            return res;
        }

        public byte[] Encrypt(string publicKey, byte[] data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);

                int maxDataLen = (rsa.KeySize / 8) - 42;
                List<byte> encrypted = new List<byte>();

                for (int i = 0; i < data.Length; i += maxDataLen)
                {
                    byte[] dataChunk = data[i..].Take(maxDataLen).ToArray();
                    byte[] encryptedChunk = rsa.Encrypt(dataChunk, false);
                    encrypted.AddRange(encryptedChunk);
                }

                return encrypted.ToArray();
            }
        }

        public byte[] DecryptFile(string filename)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                Console.WriteLine("Error! Private key is not set");
                return null;
            }

            byte[] bytes = FileWorksCrypto.ReadFile(filename);

            byte[] res = Decrypt(privateKey, bytes);

            return res;
        }

        public byte[] Decrypt(string privateKey, byte[] data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                int dataLen = rsa.KeySize / 8;
                List<byte> decrypted = new List<byte>();

                for (int i = 0; i < data.Length; i += dataLen)
                {
                    byte[] dataChunk = data[i..].Take(dataLen).ToArray();
                    byte[] decryptedChunk = rsa.Decrypt(dataChunk, false);
                    decrypted.AddRange(decryptedChunk);
                }

                return decrypted.ToArray();
            }
        }
    }

}
