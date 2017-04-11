using Xunit;

namespace Issuna.Core.Tests
{
    public class JasmineIdTests
    {
        [Fact]
        public void TestLongConstructor()
        {
            var jasmineId = new JasmineId(74547317860381854);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(8678450, jasmineId.Timestamp);
            Assert.Equal(0, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(439454, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8678450), jasmineId.CreationTime);
            Assert.Equal("74547317860381854", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeDefaultArgumentsConstructor()
        {
            var jasmineId = new JasmineId(0, 8678450, 0, 0, 439454);
            Assert.Equal(0, jasmineId.Reserved);
            Assert.Equal(8678450, jasmineId.Timestamp);
            Assert.Equal(0, jasmineId.Region);
            Assert.Equal(0, jasmineId.Machine);
            Assert.Equal(439454, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8678450), jasmineId.CreationTime);
            Assert.Equal("74547317860381854", jasmineId.ToString());
        }

        [Fact]
        public void TestFullSizeCustomizedArgumentsConstructor()
        {
            var jasmineId = new JasmineId(1, 8676875, 5, 256, 23456);
            Assert.Equal(1, jasmineId.Reserved);
            Assert.Equal(5, jasmineId.Region);
            Assert.Equal(256, jasmineId.Machine);
            Assert.Equal(8676875, jasmineId.Timestamp);
            Assert.Equal(23456, jasmineId.Sequence);
            Assert.Equal(JasmineId.JasmineIdTimer.JasmineIdEpoch.AddSeconds(8676875), jasmineId.CreationTime);
            Assert.Equal("-9148838242504647776", jasmineId.ToString());
        }

        [Fact]
        public void TestParse()
        {
            var jasmineId1 = JasmineId.Parse("74547317860381854");
            Assert.True(jasmineId1.ToLong().Equals(74547317860381854));
            Assert.True(jasmineId1.ToString() == "74547317860381854");

            var jasmineId2 = JasmineId.Parse("-9148838242504647776");
            Assert.True(jasmineId2.ToLong().Equals(-9148838242504647776));
            Assert.True(jasmineId2.ToString() == "-9148838242504647776");
        }
    }
}
