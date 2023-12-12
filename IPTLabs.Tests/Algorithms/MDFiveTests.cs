using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;

namespace IPTLabs.Tests.Algorithms
{
    public class MDFiveTests
    {
        private string message = "Hello there";
        private System.Security.Cryptography.MD5 md5;

        [SetUp]
        public void SetUp()
        {
            md5 = System.Security.Cryptography.MD5.Create();
        }

        [Test]
        public void HashText_Test()
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(message);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            string ExpectedHash = Convert.ToHexString(hashBytes);

            string ResultedHash = MDFive.HashText(message).ToUpper();

            Assert.AreEqual(ExpectedHash, ResultedHash);
        }
    }
}
