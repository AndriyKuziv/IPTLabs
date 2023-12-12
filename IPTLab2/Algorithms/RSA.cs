using IPTLab2.Data;
using IPTLab2.FileWorks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.Algorithms
{
    public class RSA
    {
        private static int keySize = 4096;
        private static string keyPairName = "keyPair.json";

        private string publicKey;
        private string privateKey;

        public RSA()
        {
            ReadKeys(keyPairName);
            if (publicKey is null || privateKey is null)
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
            KeyPair? pars = JsonConvert.DeserializeObject<KeyPair>(json);
            if (pars is null)
            {
                Console.WriteLine("Warning! Some keys are missing");
            }
            publicKey = pars.publicKey;
            privateKey = pars.privateKey;

            Console.WriteLine("\n| Public and private keys have been set |");
        }

        public void GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                privateKey = rsa.ToXmlString(true);
                publicKey = rsa.ToXmlString(false);

                SaveKeys(publicKey, privateKey);
                Console.WriteLine("\n| New public and private keys have been generated and saved |");
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

            FileWorksCrypto.SaveJsonFile(new KeyPair
            {
                privateKey = privateKey,
                publicKey = publicKey
            }, name);
        }

        public byte[] EncryptFile(string filename)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                Console.WriteLine("Error! Public key is not set");
                return null;
            }

            byte[] bytes = FileWorksCrypto.ReadFile(filename);

            // Getting a signature
            byte[] bytesHash = Encoding.UTF8.GetBytes(MDFive.HashArray(bytes));
            byte[] signature = DigitSign.SignData(bytesHash, privateKey);
            Console.WriteLine("File signature: " + Convert.ToBase64String(signature));

            // Getting bytes of a length of a signature
            int len = signature.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBytes);
            }

            byte[] ct = Encrypt(publicKey, bytes);

            // Creating a final bytes array
            byte[] res = new byte[lenBytes.Length + signature.Length + ct.Length];

            lenBytes.CopyTo(res, 0);
            signature.CopyTo(res, lenBytes.Length);
            ct.CopyTo(res, lenBytes.Length + signature.Length);

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

            // Getting a length of a signature
            byte[] lenBytes = bytes[0..4];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBytes);
            }

            int len = BitConverter.ToInt32(lenBytes, 0);

            // Getting a signature
            int endIndex = len + 4;
            byte[] signature = bytes[4..endIndex];
            Console.WriteLine("Signature from an encrypted file: " + Convert.ToBase64String(signature));

            byte[] res = Decrypt(privateKey, bytes[endIndex..]);

            byte[] resHash = Encoding.UTF8.GetBytes(MDFive.HashArray(res));
            var isValidated = DigitSign.VerifySignature(resHash, signature, publicKey);
            Console.WriteLine("| Is Validated: {0} |", isValidated);

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
