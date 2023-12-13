using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace IPTLabs.Tests.Algorithms
{
    [TestFixture]
    public class RSATests
    {
        private IPTLab2.Algorithms.RSA rsa { get; set; }

        private string testFilename = "rcf.txt";
        private string keyPairFileName = "keyPair.json";

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


        [TearDown]
        public void TearDown()
        {
            // Clean up any generated key pair and test file after each test
            File.Delete(keyPairFileName);
            File.Delete(testFilename + ".rsa");
            File.Delete("RSADecoded_" + testFilename);
        }

        [Test]
        public void GenerateKeysAndReadKeys_ValidInput_KeysAreSet()
        {
            // Arrange
            RSA rsa = new RSA();

            // Act
            rsa.GenerateKeys();
            rsa.ReadKeys(keyPairFileName);

            // Assert
            Assert.IsNotNull(rsa.PublicKey);
            Assert.IsNotNull(rsa.PrivateKey);
        }
    }
}
