using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;

namespace IPTLabs.Tests.Algorithms
{
    public class DigitSignTests
    {

        private string testFilename = "rcf.txt";
        private string testFilename2 = "keyPair.json";

        private string privateKey;
        private string publicKey;

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Test()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filePath = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename);

            var origArr = File.ReadAllBytes(filePath);

            string filePath2 = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename2);

            using StreamReader r = new StreamReader(filePath2);
            var json = r.ReadToEnd();
            KeyPair? pars = JsonConvert.DeserializeObject<KeyPair>(json);

            Assert.NotNull(pars);

            publicKey = pars.publicKey;
            privateKey = pars.privateKey;

            var signature = DigitSign.SignData(origArr, privateKey);

            var isValidated = DigitSign.VerifySignature(origArr, signature, publicKey);

            Assert.True(isValidated);
        }
    }
}
