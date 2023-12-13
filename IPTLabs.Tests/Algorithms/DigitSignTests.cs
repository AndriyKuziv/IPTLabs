using IPTLab2.Data;
using IPTLab2.Algorithms;
using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IPTLabs.Tests.Algorithms
{
    [TestFixture]
    public class DigitSignTests
    {

        private string testFilename = "rcf.txt";
        string testText = "Hello there";

        private string privateKey = "<RSAKeyValue><Modulus>wHvhiCSNdVlNtBIXcwG9tYW3mFnq9LWK8WfEV44YW2Nt4/HycQG4ZooQDJOeS6Z3l6kIICKr6P2rxePrqi2T/CZbBh59OXV6F5w6m8pFoIt0U9VNDonqMV4+CfeG6l7zCp5P1ZVp2w0GAWC8Ba57VZjZGibvuaxW9HkRA40QkFT/V8DgHfQFO45LhZCvt1KxY8hKJvSFxfgOoetjrN+Xfbig58HSTy5xZYkvxllW21VUx24mmKj6UhtrFcGHwclEBkbYxvG82OGMFamCmif6rXzmRDimvV3QuD1YE2hbruQShxXu9YKWJGS2ReOG0HTEkwmAh3QzqUI0PkqjEPcPqyGov+4eY+EcASIZk5IM68h+yLnjpq5qOi1vGlI76SeFDPSmyeG8/siIezVgLQOo8+y67dwUijIrMt9EGu5dXyRldTmUqtecpzLHQ0YrSYsEo8dIvX8+tmfdVMBzKFqIpKkx14LrD99F+V3JlG9kjVKlEgt5ANUyvhDEH5oCgIklNJoAOyRYjNjK7poHwyt1AVd6vAeSSyH8aEh/JFc8yWLsADfbMFy+q2ZMCAqSxRHGRqJH6IJpfpuezuvFNHAuzFmJmfumqDvr7IPeLa3q7BGWsZtlanzPeQ1DP6UJ7fMo/f84LXahufpEpuEV+95P3U9ik4xBwczYFVZD/ax9AAE=</Modulus><Exponent>AQAB</Exponent><P>0eWRVZlC8uSziCQy41nHG/4+9MDT/yXeAFCqQL2+qzLroiwuDtZgIZ8ouMLeW65o4EO8s/kl0OIpiSVH7aJHvQWS7BgQJLc7x7ltEnqpqA5xB4iZy/qj4LJUyvUFDHIFdSoCcUjFm6tDTioETFZLnvqGsKHRdb6zvCLnH1NMLG/95vtDo4cOF/LjbdZ1Iaz1cJ2Y0VhFAewYRzLAcpE5DuelC1pV7xprCgWfKp1Qhv5E+ZheeyHsIIi3KY/vTg8jmWkrOp6afOr6kD6QKYq2oLzjA2RhN7r62ygeVrb3VEriATANej5HqEKxUu//3b41oQaaP7m1xNR/8zsSQxK4Iw==</P><Q>6sMxyY23qRaRNs11lMSkw0lAcwl/Bm2FmKKMeqyMKAKbueSi5KTtdWN0ZsZws7irG6caw6Pqfjw3ytmQkfY9kqDpIEN515pOhrisSOH7KqBC9SJik2MtOJvlXhDbLJUkMIci78D1bHSwkbqpJuj4K0JN4OB5SaWRffZxbA4XDT6dv8GysBVK0U0fAyNxcpkFAAG/AYPeJ6DwagCeP/g6tH4KfHh41u41c8dDfpVyLRDm7hR8emlJvH8UmOYkmf2K38XUNShPgscGn7/SFJ4CkoBIdVncmZi3XiLcv3Yr9AvYDvhnoRbzKpIpBY656uXrwAZjb3EOFCJqNNYfs6a3iw==</Q><DP>ZHaEwXvB15AvfhIVaUwnSPbAG1XQImd1gqEZXDN1d2u/7LdItEFXccBENWrQEwG/oNfLEjlGKFyXlNULQGpT+90XHXnHAEvTiRzvX9DmM1H2N6ziMAefOIUioKH18KcChbm9wYsbSg8G//qMZB1JxKYUoaZSIxFL5diu1wd/hiV0WYIaNcMGs9/Fi2UCW0P1lhlYc5rULL9cP3fDNRLdRI0LKgvq5a63XOyj6jmxuWsqiGytjDIIWuoVAQereqk0WNFFp6ESQIgXtcK8LaX/E6BU5+K7xXjxaMQjAU5Bw060Le1aBhCZK6gAR7ontXK9KH2IhsfUgIlVVI4Jh6EGEQ==</DP><DQ>cnTYQTwm3vlxsxZYzT9SVSPRmER4+dlL2S7m7qhRbPBYnDksce03GJR0m+cD65uNUN4X3mp8WS9ixDNumLtFKcfp4SEEtmk0/9nppV8H5bBc9Mbe6Jzh30eifEobkZDlJAO+tMWO4mHB9ErdTWsoE28wKQNIHu/qD0+n3NbBxmNQ5by6Mb8vyvuesxkEpqqUFXzEcyinlwqQLB5BCU8sz/LjievyfleFF/1+mqiiEfa7oDe8uLG8kXojwKf0EF/c5Vy+KWyN12TRhfEvq32eI28H5K77LqeWPv8gQPiMVV+w5xuU6qUVyRuZhGwhmLSgS+7Ra/PiuU5OGZSLwDr4IQ==</DQ><InverseQ>u0d+i5h4VP0yLBiOzeOACIz7l4uZCp0iR4g4J7nMfLc9JopzFJZYJhF4sHh71XHFitO46mGEALlJ5HcGlEkq7joN66Jvy1t/aCrVfSjlLCx5tPSxmPew60bYHPn5SXx93mjNw+sIutqnSTuMMm2GcGslAopRdjW50VZ9NzIGSaPn6TPu4NxLMSfdkCXJN+mmMbHQilzLZrRPAwzMzO3s3LzIveCI41rjXcET+YCj7yzoIcjfie2NtsfrR8cGlVHseQrAC5H1ZYsAOdOI63nfux5RA27e40lFtm95isdwGm48ck15Fxvl89h90SCoIiQH11lyeVCD+snSnzsa2wPkhg==</InverseQ><D>mlE3SWz0tEY6k8mFX0DgFW30+YHtau85S71DnmZfhLU8XumU06Drc9nwcATRfhkh/EcaKplhddQaKPdH8xHDeP5PL6K2BqD2l4M7QbdzBvmG88coliVfyuChoGAAnFovRC4UF0pIqcPRMr2ZQHzYiJGYTO02eIeA11NQnqYzx2M9q98ITztaDptQVO7g0XrMmRD00jJsJYFwScRsDybbT4H8DpJHyA+V/i90S/NIPWQ2KgVXkEC5H6a3vaOA8Y4oqVeXaDhajQ6Wvq8989Vllj1yZvN8SomyUmNr/3NFPfvAFukwwUhGkzrjHzkA1pMnHjMaBn7VBDf9pos0HN/WgJpm2QodNRqk2iQs1sHdglM5beESSQ4t367/fEFoISW684KXihGKPUNU7e5Ya23uelmYmrtFRkUP+9dKWa5wx6tWyHLHQF5P5APzez2zGkBnNW4d971VEsJe3JTWmmOyaSsvvKCJBPnQk8qQ/9bbFg70c4UalHT7KcoA9MH3zYXtsUYmgj9PA9xiZl+rq1TR8RiYf8f62iTFNcAYkYYv+M+CYaIftg2cOTaY6SEeVgdIS3eb48syUuPqMjCmLZC2uXkraogFg4DJdq+x1mntKdc1suKJYLjC7W73tg9LWHiHRsucsNMRi0fUzAgeLG8vVeYIW7P7QPKLbZEGTGeXOFk=</D></RSAKeyValue>";
        private string publicKey = "<RSAKeyValue><Modulus>wHvhiCSNdVlNtBIXcwG9tYW3mFnq9LWK8WfEV44YW2Nt4/HycQG4ZooQDJOeS6Z3l6kIICKr6P2rxePrqi2T/CZbBh59OXV6F5w6m8pFoIt0U9VNDonqMV4+CfeG6l7zCp5P1ZVp2w0GAWC8Ba57VZjZGibvuaxW9HkRA40QkFT/V8DgHfQFO45LhZCvt1KxY8hKJvSFxfgOoetjrN+Xfbig58HSTy5xZYkvxllW21VUx24mmKj6UhtrFcGHwclEBkbYxvG82OGMFamCmif6rXzmRDimvV3QuD1YE2hbruQShxXu9YKWJGS2ReOG0HTEkwmAh3QzqUI0PkqjEPcPqyGov+4eY+EcASIZk5IM68h+yLnjpq5qOi1vGlI76SeFDPSmyeG8/siIezVgLQOo8+y67dwUijIrMt9EGu5dXyRldTmUqtecpzLHQ0YrSYsEo8dIvX8+tmfdVMBzKFqIpKkx14LrD99F+V3JlG9kjVKlEgt5ANUyvhDEH5oCgIklNJoAOyRYjNjK7poHwyt1AVd6vAeSSyH8aEh/JFc8yWLsADfbMFy+q2ZMCAqSxRHGRqJH6IJpfpuezuvFNHAuzFmJmfumqDvr7IPeLa3q7BGWsZtlanzPeQ1DP6UJ7fMo/f84LXahufpEpuEV+95P3U9ik4xBwczYFVZD/ax9AAE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void SignData_Test()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filePath = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename);

            var origArr = File.ReadAllBytes(filePath);

            var signature = DigitSign.SignData(origArr, privateKey);
            Assert.NotNull(signature);
        }

        [Test]
        public void VerifyData_Test()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string filePath = Path.Combine(projectPath, "Algorithms", "TestFiles", testFilename);

            var origArr = File.ReadAllBytes(filePath);

            var signature = DigitSign.SignData(origArr, privateKey);
            Assert.NotNull(signature);

            var isValidated = DigitSign.VerifySignature(origArr, signature, publicKey);
            Assert.NotNull(isValidated);

            Assert.True(isValidated);
        }

        [Test]
        public void GenerSignForString_ReturnsSignature()
        {
            byte[] signature = DigitSign.GenerSignForString(testText, privateKey);

            Assert.IsNotNull(signature);
            Assert.IsTrue(signature.Length > 0);
        }

        [Test]
        public void SignData_ReturnsSignature()
        {
            byte[] data = Encoding.UTF8.GetBytes(testText);

            byte[] signature = DigitSign.SignData(data, privateKey);

            Assert.IsNotNull(signature);
            Assert.IsTrue(signature.Length > 0);
        }

        [Test]
        public void VerifySignature_ReturnsTrue()
        {
            byte[] data = Encoding.UTF8.GetBytes(testText);
            byte[] validSignature = DigitSign.SignData(data, privateKey);

            bool isSignatureValid = DigitSign.VerifySignature(data, validSignature, publicKey);

            Assert.IsTrue(isSignatureValid);
        }

        [Test]
        public void VerifySignature_ReturnsFalse()
        {
            byte[] data = Encoding.UTF8.GetBytes(testText);
            byte[] invalidSignature = new byte[] { 1, 2, 3 };

            bool isSignatureValid = DigitSign.VerifySignature(data, invalidSignature, publicKey);

            Assert.IsFalse(isSignatureValid);
        }
    }
}
