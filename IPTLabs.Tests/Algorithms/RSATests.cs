using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace IPTLabs.Tests.Algorithms
{
    public class RSATests
    {
        private IPTLab2.Algorithms.RSA rsa { get; set; }

        private string testFilename = "rcf.txt";

        [SetUp]
        public void SetUp()
        {
            rsa = new IPTLab2.Algorithms.RSA();
            rsa.GenerateKeys();
        }

        [Test]
        public void EncryptDecrypt_Test()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filePath = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename);

            var origArr = File.ReadAllBytes(filePath);

            var encryptedArr = rsa.ExecuteEncryption(origArr);

            var decryptedArr = rsa.ExecuteDecryption(encryptedArr);

            Assert.That(decryptedArr, Is.EqualTo(origArr));
        }
    }
}
