using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace IPTLabs.Tests.Algorithms
{
    [TestFixture]
    public class RCFiveTests
    {
        private GeneratorParams _genParams { get; set; }
        private RCFiveParams _rcparams { get; set; }
        private System.Security.Cryptography.MD5 md5 { get; set; }
        private RCFive rcf {  get; set; }

        private string password = "123";
        private string testFilename = "rcf.txt";


        [SetUp]
        public void SetUp()
        {
            _rcparams = new RCFiveParams();
            _rcparams.rounds = 20;
            _rcparams.MaxFileSize = 10;
            _rcparams.keySize = 16;
            _rcparams.wordSize = 32;

            _genParams = new GeneratorParams();

            md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            rcf = new RCFive(Convert.ToHexString(hashBytes), _rcparams.wordSize, _rcparams.rounds, _rcparams.keySize);
        }

        [Test]
        public void RCFiveTest()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filePath = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename);

            var origArr = File.ReadAllBytes(filePath);

            _genParams.a = 8;
            _genParams.x0 = 256;
            _genParams.c = 21;
            _genParams.m = 131071;

            var encryptedFile = rcf.ExecuteEncryption(origArr, _genParams);
            Assert.IsNotNull(encryptedFile);

            var decryptedFile = rcf.ExecuteDecryption(encryptedFile);

            Assert.IsNotNull(decryptedFile);
            Assert.That(decryptedFile.Take(origArr.Length).ToArray(), Is.EqualTo(origArr));
        }
    }
}
