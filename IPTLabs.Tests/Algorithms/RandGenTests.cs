using IPTLab2.Data;
using IPTLab2.Algorithms;

namespace IPTLabs.Tests.Algorithms
{
    [TestFixture]
    public class RandGenTests
    {
        private GeneratorParams _genParams { get; set; }

        [SetUp]
        public void Setup()
        {
            _genParams = new GeneratorParams();
        }

        [Test]
        public void Gener_Test()
        {
            _genParams.a = 8;
            _genParams.x0 = 256;
            _genParams.c = 21;
            _genParams.m = 131071;

            var firstSet = RandGen.Gener(_genParams, 5);
            var secondSet = RandGen.Gener(_genParams, 40);

            var firstArray = firstSet.Key;
            var secondArray = secondSet.Key.Take(5);

            Assert.AreEqual(firstArray, secondArray);
        }

        [Test]
        public void GenerBytes_Test()
        {
            _genParams.a = 8;
            _genParams.x0 = 256;
            _genParams.c = 21;
            _genParams.m = 131071;

            var firstArr = RandGen.GenerBytes(_genParams, 5);
            var secondArr = RandGen.GenerBytes(_genParams, 5);

            Assert.AreEqual(firstArr, secondArr);
        }
    }
}
