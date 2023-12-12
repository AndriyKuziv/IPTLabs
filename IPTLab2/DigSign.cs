using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2
{

    public class DigSign
    {
        public static void start()
        {
            // Create a new instance of DSACryptoServiceProvider
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // Generate a key pair (public and private key)
                RSAParameters privateKey = rsa.ExportParameters(true);
                RSAParameters publicKey = rsa.ExportParameters(false);

                // Message to be signed
                string messageToSign = "Hello, Digital Signature!";

                // Convert the message to a byte array
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSign);

                // Create a digital signature
                byte[] signature = SignData(messageBytes, rsa.ToXmlString(true));

                Console.WriteLine("Original Message: " + messageToSign);
                Console.WriteLine("Digital Signature: " + Convert.ToBase64String(signature));

                // Verify the signature
                bool isSignatureValid = VerifySignature(messageBytes, signature, publicKey);
                Console.WriteLine("Signature Verification: " + isSignatureValid);
            }
        }

        public static byte[] GenerSignForString(string text, string privateKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            return SignData(data, privateKey);
        }

        public static byte[] SignData(byte[] data, RSAParameters privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);

                byte[] signature = rsa.SignData(data, new SHA256CryptoServiceProvider());
                return signature;
            }
        }

        public static byte[] SignData(byte[] data, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);

                byte[] signature = rsa.SignData(data, new SHA256CryptoServiceProvider());
                return signature;
            }
        }

        public static bool VerifySignature(byte[] data, byte[] signature, RSAParameters publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);

                bool isSignatureValid = rsa.VerifyData(data, new SHA256CryptoServiceProvider(), signature);
                return isSignatureValid;
            }
        }

        public static bool VerifySignature(byte[] data, byte[] signature, string publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);

                bool isSignatureValid = rsa.VerifyData(data, new SHA256CryptoServiceProvider(), signature);
                return isSignatureValid;
            }
        }
    }
}
